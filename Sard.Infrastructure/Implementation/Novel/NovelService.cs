namespace Sard.Infrastructure.Implementation.Novel
{
    public class NovelService(AppDbContext db) : INovelService
    {
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

            return Result<NovelSummaryDto>.Success(MapToDto(novel));
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

            return Result<string>.Success("تم الحفظ");
        }

        private static NovelSummaryDto MapToDto(Sard.Domain.Entities.Novel n) =>
            new(n.Id, n.Title, n.Description, n.CoverImageUrl, n.Price, n.Status,
                n.Chapters?.Count ?? 0, n.LastReadChapterId, n.CreatedAt);

        private static ChapterDto MapChapterToDto(Chapter c) =>
            new(c.Id, c.Title, c.Content, c.Order, c.LastEditedAt);
    }
}
