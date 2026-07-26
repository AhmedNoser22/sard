namespace Sard.Application.DTOs.Admin
{
    public record AdminNovelDto(
    int Id,
    string Title,
    string AuthorName,
    string? CoverImageUrl,
    decimal Price,
    int ChaptersCount,
    int ReadCount,
    DateTime CreatedAt
);
}
