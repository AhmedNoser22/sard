namespace Sard.Application.DTOs.Auth
{
    public record AuthResponseDto(
    string? Id,
    string DisplayName,
    string Email,
    string Token
);
}
