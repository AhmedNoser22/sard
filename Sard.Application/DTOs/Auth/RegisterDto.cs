namespace Sard.Application.DTOs.Auth
{
    public record RegisterDto(
        string DisplayName,
        string Email,
        string Password,
        string ConfirmPassword,
        bool AgreeToTerms
    );
}