namespace Sard.Infrastructure.Settings
{
    public class StripeSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public decimal PlatformFeePercent { get; set; } = 10;
    }
}