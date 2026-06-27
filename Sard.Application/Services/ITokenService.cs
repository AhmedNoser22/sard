namespace Sard.Application.Services
{
    public interface ITokenService
    {
        string GenerateToken(AppUser user, IList<string> roles);
    }
}
