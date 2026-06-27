namespace Sard.Application.DTOs.Auth
{
    public record ConfirmEmailDto(
    string Email,
    string Code
        );
}
