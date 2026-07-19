using Stripe;
using Stripe.Checkout;

namespace Sard.Infrastructure.Services
{
    public class StripeService(
        IOptions<StripeSettings> options,
        AppDbContext db) : IPaymentService
    {
        private readonly StripeSettings _settings = options.Value;

        public async Task<Result<string>> InitiatePublishPaymentAsync(string userId, int novelId)
        {
            var novel = await db.Novels
                .Include(n => n.Author)
                .FirstOrDefaultAsync(n => n.Id == novelId && n.AuthorId == userId);

            if (novel is null)
                return Result<string>.Failure("الرواية غير موجودة");

            if (novel.Status == NovelStatus.Published)
                return Result<string>.Failure("الرواية منشورة بالفعل");

            var platformFeeCents = novel.Price > 0
                ? (long)(novel.Price * _settings.PlatformFeePercent / 100 * 100)
                : 500;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "egp",
                            UnitAmount = platformFeeCents,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"نشر رواية: {novel.Title}",
                                Description = "رسوم نشر الرواية على منصة سرد"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = "http://localhost:4200/payment-success",
                CancelUrl = "http://localhost:4200/payment-failed",
                Metadata = new Dictionary<string, string>
                {
                    { "novelId", novelId.ToString() },
                    { "userId", userId }
                }
            };

            var service = new SessionService();
            Session session;
            try
            {
                session = await service.CreateAsync(options, new RequestOptions { ApiKey = _settings.SecretKey });
            }
            catch (StripeException ex)
            {
                return Result<string>.Failure($"فشل إنشاء عملية الدفع: {ex.Message}");
            }

            novel.Status = NovelStatus.PendingPayment;
            await db.SaveChangesAsync();

            return Result<string>.Success(session.Url);
        }

        public async Task<Result<string>> HandleWebhookAsync(string json, string stripeSignature)
        {
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _settings.WebhookSecret);
            }
            catch (Exception)
            {
                return Result<string>.Failure("توقيع Webhook غير صحيح");
            }

            if (stripeEvent.Type != "checkout.session.completed")
                return Result<string>.Success("ignored");

            var session = stripeEvent.Data.Object as Session;
            if (session is null)
                return Result<string>.Failure("بيانات غير صحيحة");

            if (!session.Metadata.TryGetValue("novelId", out var novelIdStr) ||
                !int.TryParse(novelIdStr, out var novelId))
                return Result<string>.Failure("بيانات غير صحيحة");

            var novel = await db.Novels.FindAsync(novelId);
            if (novel is null)
                return Result<string>.Failure("الرواية غير موجودة");

            novel.Status = NovelStatus.Published;

            db.Purchases.Add(new Purchase
            {
                UserId = novel.AuthorId,
                NovelId = novel.Id,
                Amount = (session.AmountTotal ?? 0) / 100m,
                PaymobTransactionId = session.Id,
                Type = PurchaseType.PublishFee,
                PaidAt = EgyptDateTime.Now
            });

            await db.SaveChangesAsync();
            return Result<string>.Success("تم النشر");
        }
    }
}