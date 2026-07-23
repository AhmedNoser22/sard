namespace Sard.Application.Interfaces.Novel
{
    public interface INovelService
    {
        Task<Result<NovelSummaryDto>> CreateNovelAsync(string userId, CreateNovelDto dto);
        Task<Result<string>> UploadCoverAsync(string userId, int novelId, IFormFile cover);
        Task<Result<NovelSummaryDto>> UpdateNovelAsync(string userId, int novelId, UpdateNovelDto dto);
        Task<Result<ChapterDto>> SaveChapterAsync(string userId, int novelId, SaveChapterDto dto, int? chapterId);
        Task<Result<ChapterDto>> GetChapterAsync(string userId, int novelId, int chapterId);
        Task<Result<IEnumerable<ChapterDto>>> GetChaptersAsync(string userId, int novelId);
        Task<Result<string>> SetLastReadChapterAsync(string userId, int novelId, int chapterId);
        Task<Result<NovelSummaryDto>> UpdateNovelSettingsAsync(string userId, int novelId, UpdateNovelSettingsDto dto);
        Task<Result<IEnumerable<PublishedNovelDto>>> GetPublishedNovelsAsync(int page, int pageSize);
        Task<Result<bool>> HasPurchasedAsync(string userId, int novelId);
        Task<Result<NovelDownloadDto>> GetNovelForDownloadAsync(string userId, int novelId);
        Task<Result<IEnumerable<PublishedNovelDto>>> GetPurchasedNovelsAsync(string userId);
        Task<Result<string>> ConfirmPurchaseAsync(string userId, int novelId, string sessionId);
    }
}
