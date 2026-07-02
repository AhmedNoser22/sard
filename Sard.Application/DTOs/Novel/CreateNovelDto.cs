namespace Sard.Application.DTOs.Novel
{
    public record CreateNovelDto(
    string Title,
    string? Description,
    decimal Price
);
}
