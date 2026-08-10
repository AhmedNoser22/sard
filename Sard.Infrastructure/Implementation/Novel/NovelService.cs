namespace Sard.Infrastructure.Implementation.Novel
{
    public class NovelService(AppDbContext db, IImageService imageService, ICacheService cache) : INovelService
    {
        private static string ProfileCacheKey(string userId) => $"profile:{userId}";

        public async Task<Result<NovelSummaryDto>> CreateNovelAsync(string userId, CreateNovelDto dto)
        {
            var novel = new Sard.Domain.Entities.Novel
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                AuthorId = userId,
                Status = NovelStatus.Draft,
                CreatedAt = EgyptDateTime.Now
            };

            db.Novels.Add(novel);
            await db.SaveChangesAsync();
            await cache.RemoveAsync(ProfileCacheKey(userId));

            return Result<NovelSummaryDto>.Success(MapToDto(novel));
        }

        public async Task<Result<string>> DeleteNovelAsync(string userId, int novelId)
        {
            var novel = await db.Novels
                .Include(n => n.Chapters)
                .FirstOrDefaultAsync(n => n.Id == novelId && n.AuthorId == userId);

            if (novel is null)
                return Result<string>.Failure("الرواية غير موجودة");

            if (!string.IsNullOrEmpty(novel.CoverImageUrl))
                await imageService.DeleteAsync(novel.CoverImageUrl);

            db.Chapters.RemoveRange(novel.Chapters);
            db.Novels.Remove(novel);

            await db.SaveChangesAsync();
            var cacheKey = ProfileCacheKey(userId);
            await cache.RemoveAsync(cacheKey);

            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                await cache.RemoveAsync(cacheKey);
            });

            return Result<string>.Success("تم الحذف بنجاح");
        }

        public async Task<Result<string>> UploadCoverAsync(string userId, int novelId, IFormFile cover)
        {
            var novel = await db.Novels
                .FirstOrDefaultAsync(n => n.Id == novelId && n.AuthorId == userId);

            if (novel is null)
                return Result<string>.Failure("الرواية غير موجودة");

            if (!string.IsNullOrEmpty(novel.CoverImageUrl))
                await imageService.DeleteAsync(novel.CoverImageUrl);

            var upload = await imageService.UploadAsync(cover, "sard/covers");
            if (!upload.IsSuccess)
                return Result<string>.Failure(upload.Error!);

            novel.CoverImageUrl = upload.Data;
            await db.SaveChangesAsync();
            await cache.RemoveAsync(ProfileCacheKey(userId));

            return Result<string>.Success(upload.Data!);
        }

        public async Task<Result<NovelSummaryDto>> UpdateNovelAsync(string userId, int novelId, UpdateNovelDto dto)
        {
            var novel = await db.Novels
                .Include(n => n.Chapters)
                .FirstOrDefaultAsync(n => n.Id == novelId && n.AuthorId == userId);

            if (novel is null)
                return Result<NovelSummaryDto>.Failure("الرواية غير موجودة");

            novel.Title = dto.Title;
            novel.Description = dto.Description;
            novel.Price = dto.Price;

            await db.SaveChangesAsync();
            await cache.RemoveAsync(ProfileCacheKey(userId));

            return Result<NovelSummaryDto>.Success(MapToDto(novel));
        }

        public async Task<Result<ChapterDto>> SaveChapterAsync(string userId, int novelId, SaveChapterDto dto, int? chapterId)
        {
            var novel = await db.Novels
                .FirstOrDefaultAsync(n => n.Id == novelId && n.AuthorId == userId);

            if (novel is null)
                return Result<ChapterDto>.Failure("الرواية غير موجودة");

            Chapter chapter;

            if (chapterId.HasValue)
            {
                chapter = await db.Chapters
                    .FirstOrDefaultAsync(c => c.Id == chapterId && c.NovelId == novelId)
                    ?? new Chapter();

                chapter.Title = dto.Title;
                chapter.Content = dto.Content;
                chapter.Order = dto.Order;
                chapter.LastEditedAt = EgyptDateTime.Now;

                if (chapter.Id == 0)
                {
                    chapter.NovelId = novelId;
                    db.Chapters.Add(chapter);
                }
            }
            else
            {
                chapter = new Chapter
                {
                    Title = dto.Title,
                    Content = dto.Content,
                    Order = dto.Order,
                    NovelId = novelId,
                    LastEditedAt = EgyptDateTime.Now
                };
                db.Chapters.Add(chapter);
            }

            novel.LastReadChapterId = chapter.Id == 0 ? null : chapter.Id;
            await db.SaveChangesAsync();

            novel.LastReadChapterId = chapter.Id;
            await db.SaveChangesAsync();
            await cache.RemoveAsync(ProfileCacheKey(userId));

            return Result<ChapterDto>.Success(MapChapterToDto(chapter));
        }

        public async Task<Result<ChapterDto>> GetChapterAsync(string userId, int novelId, int chapterId)
        {
            var chapter = await db.Chapters
                .Include(c => c.Novel)
                .FirstOrDefaultAsync(c => c.Id == chapterId && c.NovelId == novelId && c.Novel.AuthorId == userId);

            if (chapter is null)
                return Result<ChapterDto>.Failure("الفصل غير موجود");

            return Result<ChapterDto>.Success(MapChapterToDto(chapter));
        }

        public async Task<Result<IEnumerable<ChapterDto>>> GetChaptersAsync(string userId, int novelId)
        {
            var novel = await db.Novels
                .FirstOrDefaultAsync(n => n.Id == novelId && n.AuthorId == userId);

            if (novel is null)
                return Result<IEnumerable<ChapterDto>>.Failure("الرواية غير موجودة");

            var chapters = await db.Chapters
                .Where(c => c.NovelId == novelId)
                .OrderBy(c => c.Order)
                .ToListAsync();

            return Result<IEnumerable<ChapterDto>>.Success(chapters.Select(MapChapterToDto));
        }

        public async Task<Result<string>> SetLastReadChapterAsync(string userId, int novelId, int chapterId)
        {
            var novel = await db.Novels
                .FirstOrDefaultAsync(n => n.Id == novelId && n.AuthorId == userId);

            if (novel is null)
                return Result<string>.Failure("الرواية غير موجودة");

            novel.LastReadChapterId = chapterId;
            await db.SaveChangesAsync();
            await cache.RemoveAsync(ProfileCacheKey(userId));

            return Result<string>.Success("تم الحفظ");
        }

        public async Task<Result<NovelSummaryDto>> UpdateNovelSettingsAsync(string userId, int novelId, UpdateNovelSettingsDto dto)
        {
            var novel = await db.Novels
                .Include(n => n.Chapters)
                .FirstOrDefaultAsync(n => n.Id == novelId && n.AuthorId == userId);

            if (novel is null)
                return Result<NovelSummaryDto>.Failure("الرواية غير موجودة");

            novel.Title = dto.Title;
            novel.Description = dto.Description;
            novel.Price = dto.Price;

            await db.SaveChangesAsync();
            await cache.RemoveAsync(ProfileCacheKey(userId));

            return Result<NovelSummaryDto>.Success(MapToDto(novel));
        }

        public async Task<Result<IEnumerable<PublishedNovelDto>>> GetPublishedNovelsAsync(int page, int pageSize)
        {
            var novels = await db.Novels
                .Where(n => n.Status == NovelStatus.Published)
                .Include(n => n.Author)
                .Include(n => n.Chapters)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result<IEnumerable<PublishedNovelDto>>.Success(novels.Select(n => new PublishedNovelDto(
                n.Id, n.Title, n.Description, n.CoverImageUrl, n.Price,
                n.AuthorId, n.Author?.DisplayName ?? "", n.Author?.ProfileImageUrl,
                n.Chapters?.Count ?? 0, n.ReadCount, n.CreatedAt
            )));
        }

        public async Task<Result<bool>> HasPurchasedAsync(string userId, int novelId)
        {
            var purchased = await db.Purchases.AnyAsync(p =>
                p.UserId == userId &&
                p.NovelId == novelId &&
                p.Type == PurchaseType.ReadFee);

            return Result<bool>.Success(purchased);
        }

        public async Task<Result<NovelDownloadDto>> GetNovelForDownloadAsync(string userId, int novelId)
        {
            var novel = await db.Novels
                .Include(n => n.Author)
                .Include(n => n.Chapters.OrderBy(c => c.Order))
                .FirstOrDefaultAsync(n => n.Id == novelId && n.Status == NovelStatus.Published);

            if (novel is null)
                return Result<NovelDownloadDto>.Failure("الرواية غير موجودة");

            if (novel.Price > 0)
            {
                var hasPurchased = await db.Purchases.AnyAsync(p =>
                    p.UserId == userId &&
                    p.NovelId == novelId &&
                    p.Type == PurchaseType.ReadFee);

                if (!hasPurchased)
                    return Result<NovelDownloadDto>.Failure("يجب شراء الرواية أولاً");
            }

            return Result<NovelDownloadDto>.Success(new NovelDownloadDto(
                novel.Title,
                novel.Author?.DisplayName ?? "",
                novel.Description,
                novel.CoverImageUrl,
                novel.Chapters.Select(c => new ChapterDownloadDto(c.Order, c.Title, c.Content))
            ));
        }

        public async Task<Result<IEnumerable<PublishedNovelDto>>> GetPurchasedNovelsAsync(string userId)
        {
            var novels = await db.Purchases
                .Where(p => p.UserId == userId && p.Type == PurchaseType.ReadFee)
                .Include(p => p.Novel).ThenInclude(n => n.Author)
                .Include(p => p.Novel).ThenInclude(n => n.Chapters)
                .Select(p => p.Novel)
                .Where(n => n != null && n.Status == NovelStatus.Published)
                .ToListAsync();

            return Result<IEnumerable<PublishedNovelDto>>.Success(novels.Select(n => new PublishedNovelDto(
                n!.Id, n.Title, n.Description, n.CoverImageUrl, n.Price,
                n.AuthorId, n.Author?.DisplayName ?? "", n.Author?.ProfileImageUrl,
                n.Chapters?.Count ?? 0, n.ReadCount, n.CreatedAt
            )));
        }

        public async Task<Result<string>> ConfirmPurchaseAsync(string userId, int novelId, string sessionId)
        {
            var alreadyPurchased = await db.Purchases.AnyAsync(p =>
                p.UserId == userId &&
                p.NovelId == novelId &&
                p.Type == PurchaseType.ReadFee);

            if (alreadyPurchased)
                return Result<string>.Success("مشتري بالفعل");

            var novel = await db.Novels.FindAsync(novelId);
            if (novel is null)
                return Result<string>.Failure("الرواية غير موجودة");

            db.Purchases.Add(new Purchase
            {
                UserId = userId,
                NovelId = novelId,
                Amount = novel.Price,
                PaymobTransactionId = sessionId,
                Type = PurchaseType.ReadFee,
                PaidAt = EgyptDateTime.Now
            });

            await db.SaveChangesAsync();
            await cache.RemoveAsync(ProfileCacheKey(userId));

            return Result<string>.Success("تم");
        }

        private static NovelSummaryDto MapToDto(Sard.Domain.Entities.Novel n) =>
            new(n.Id, n.Title, n.Description, n.CoverImageUrl, n.Price, n.Status,
                n.Chapters?.Count ?? 0, n.LastReadChapterId, n.CreatedAt);

        private static ChapterDto MapChapterToDto(Chapter c) =>
            new(c.Id, c.Title, c.Content, c.Order, c.LastEditedAt);
    }
}