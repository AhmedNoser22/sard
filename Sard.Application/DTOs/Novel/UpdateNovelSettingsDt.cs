namespace Sard.Application.DTOs.Novel
{
    public record UpdateNovelSettingsDto(
    string Title,
    string? Description,
    decimal Price
);
}
