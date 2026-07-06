namespace Sard.Application.DTOs.Profile
{
    public record FavoriteNovelDto(
    int Id,
    string Title,
    string? AuthorName,
    string? CoverImageUrl,
    decimal Price
);
}
