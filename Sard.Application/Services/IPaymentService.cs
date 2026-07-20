namespace Sard.Application.Services
{
    public interface IPaymentService
    {
        Task<Result<string>> InitiatePublishPaymentAsync(string userId, int novelId);
        Task<Result<string>> HandleWebhookAsync(string json, string stripeSignature);
        Task<Result<string>> InitiateReadPaymentAsync(string userId, int novelId);
    }
}