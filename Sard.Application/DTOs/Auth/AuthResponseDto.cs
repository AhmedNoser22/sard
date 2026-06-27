namespace Sard.Application.DTOs.Auth
{
    public record AuthResponseDto(
    string DisplayName,
    string Email,
    string Token
);
}
