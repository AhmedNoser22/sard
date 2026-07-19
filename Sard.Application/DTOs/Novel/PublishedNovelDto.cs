namespace Sard.Application.DTOs.Novel
{
    public record PublishedNovelDto(
    int Id,
    string Title,
    string? Description,
    string? CoverImageUrl,
    decimal Price,
    string AuthorId,
    string AuthorName,
    string? AuthorImageUrl,
    int ChaptersCount,
    int ReadCount,
    DateTime CreatedAt
);
}
