namespace Sard.Application.DTOs.Profile
{
    public record AddFavoriteNovelDto(
    string Title,
    string? AuthorName,
    string? CoverImageUrl
);
}
