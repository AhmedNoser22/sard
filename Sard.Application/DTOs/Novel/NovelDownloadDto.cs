namespace Sard.Application.DTOs.Novel
{
    public record NovelDownloadDto(
    string Title,
    string AuthorName,
    string? Description,
    string? CoverImageUrl,
    IEnumerable<ChapterDownloadDto> Chapters
);

    public record ChapterDownloadDto(
        int Order,
        string Title,
        string Content
    );
}
