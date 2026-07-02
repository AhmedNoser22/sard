namespace Sard.Application.DTOs.Novel
{
    public record UpdateNovelDto(
    string Title,
    string? Description,
    decimal Price
);
}
