namespace Sard.Application.DTOs.Profile
{
    public record ProfileDto(
    string Id,
    string DisplayName,
    string? Bio,
    string? ProfileImageUrl,
    DateTime CreatedAt,
    int PublishedNovelsCount,
    int TotalReadsCount,
    IEnumerable<NovelSummaryDto> Novels,
    IEnumerable<HighlightDto> Highlights
);
}
