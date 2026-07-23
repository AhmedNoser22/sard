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
                ? Math.Max(3000, (long)(novel.Price * _settings.PlatformFeePercent / 100 * 100))
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

            if (!session.Metadata.TryGetValue("userId", out var userId))
                return Result<string>.Failure("بيانات غير صحيحة");

            session.Metadata.TryGetValue("type", out var type);

            var novel = await db.Novels.FindAsync(novelId);
            if (novel is null)
                return Result<string>.Failure("الرواية غير موجودة");

            if (type == "read")
            {
                var alreadyPurchased = await db.Purchases.AnyAsync(p =>
                    p.UserId == userId &&
                    p.NovelId == novelId &&
                    p.Type == PurchaseType.ReadFee);

                if (!alreadyPurchased)
                {
                    db.Purchases.Add(new Purchase
                    {
                        UserId = userId,
                        NovelId = novelId,
                        Amount = (session.AmountTotal ?? 0) / 100m,
                        PaymobTransactionId = session.Id,
                        Type = PurchaseType.ReadFee,
                        PaidAt = EgyptDateTime.Now
                    });
                }
            }
            else
            {
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
            }

            await db.SaveChangesAsync();
            return Result<string>.Success("تم");
        }
        public async Task<Result<string>> InitiateReadPaymentAsync(string userId, int novelId)
        {
            var novel = await db.Novels
                .Include(n => n.Author)
                .FirstOrDefaultAsync(n => n.Id == novelId && n.Status == NovelStatus.Published);

            if (novel is null)
                return Result<string>.Failure("الرواية غير موجودة");

            if (novel.Price == 0)
                return Result<string>.Failure("الرواية مجانية");

            if (novel.Price > 0 && novel.Price < 30)
                return Result<string>.Failure("الحد الأدنى لسعر الرواية 30 جنيه");

            var alreadyPurchased = await db.Purchases.AnyAsync(p =>
                p.UserId == userId &&
                p.NovelId == novelId &&
                p.Type == PurchaseType.ReadFee);

            if (alreadyPurchased)
                return Result<string>.Failure("اشتريت الرواية بالفعل");

            var amountCents = (long)(novel.Price * 100);
            var description = string.IsNullOrWhiteSpace(novel.Description)
                ? $"رواية بقلم {novel.Author?.DisplayName}"
                : novel.Description;

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
                    UnitAmount = amountCents,
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = novel.Title,
                        Description = description
                    }
                },
                Quantity = 1
            }
        },
                Mode = "payment",
                SuccessUrl = $"http://localhost:4200/payment-success?novelId={novelId}&type=read&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = "http://localhost:4200/payment-failed",
                Metadata = new Dictionary<string, string>
        {
            { "novelId", novelId.ToString() },
            { "userId", userId },
            { "type", "read" }
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

            return Result<string>.Success(session.Url);
        }
    }
}