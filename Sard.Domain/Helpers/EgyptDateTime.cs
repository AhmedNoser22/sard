namespace Sard.Domain.Helpers
{
    public static class EgyptDateTime
    {
        private static readonly TimeZoneInfo EgyptZone =
            TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");

        public static DateTime Now =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EgyptZone);
    }
}
