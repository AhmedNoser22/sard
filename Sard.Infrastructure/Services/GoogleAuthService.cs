namespace Sard.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        public async Task<GooglePayload?> VerifyAsync(string idToken)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
                return new GooglePayload(payload.Email, payload.Name);
            }
            catch
            {
                return null;
            }
        }
    }
}
