namespace Sard.Application.DTOs.Profile
{
    public record HighlightDto(
    int Id,
    string Content,
    string? NovelTitle,
    string? NovelAuthor,
    DateTime CreatedAt
);
}
