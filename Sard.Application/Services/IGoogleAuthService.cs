namespace Sard.Application.Services
{
    public interface IGoogleAuthService
    {
        Task<GooglePayload?> VerifyAsync(string idToken);
    }
}
