namespace Sard.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto);
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
        Task<Result<AuthResponseDto>> GoogleLoginAsync(GoogleLoginDto dto);
        Task<Result<AuthResponseDto>> ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<Result<string>> ResendCodeAsync(ResendCodeDto dto);
        Task<Result<string>> ForgotPasswordAsync(ResendCodeDto dto);
        Task<Result<string>> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
