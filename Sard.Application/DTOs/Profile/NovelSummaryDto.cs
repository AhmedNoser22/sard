using Sard.Domain.Enums;

namespace Sard.Application.DTOs.Profile
{
    public record NovelSummaryDto(
    int Id,
    string Title,
    string? Description,
    string? CoverImageUrl,
    decimal Price,
    NovelStatus Status,
    int ChaptersCount,
    int? LastReadChapterId,
    DateTime CreatedAt
);
}
