namespace Sard.Infrastructure.Implementation.Profile
{
    public class ProfileService(
    AppDbContext db,
    UserManager<AppUser> userManager,
    IImageService imageService) : IProfileService
    {
        public async Task<Result<ProfileDto>> GetProfileAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return Result<ProfileDto>.Failure("المستخدم غير موجود");

            var novels = await db.Novels
                .Where(n => n.AuthorId == userId)
                .Include(n => n.Chapters)
                .ToListAsync();

            var highlights = await db.Highlights
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            return Result<ProfileDto>.Success(MapToDto(user, novels, highlights));
        }

        public async Task<Result<ProfileDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return Result<ProfileDto>.Failure("المستخدم غير موجود");

            user.DisplayName = dto.DisplayName;
            user.Bio = dto.Bio;

            await userManager.UpdateAsync(user);

            return await GetProfileAsync(userId);
        }

        public async Task<Result<string>> UpdateProfileImageAsync(string userId, IFormFile image)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return Result<string>.Failure("المستخدم غير موجود");

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                await imageService.DeleteAsync(user.ProfileImageUrl);

            var upload = await imageService.UploadAsync(image, "sard/profiles");
            if (!upload.IsSuccess)
                return Result<string>.Failure(upload.Error!);

            user.ProfileImageUrl = upload.Data;
            await userManager.UpdateAsync(user);

            return Result<string>.Success(upload.Data!);
        }

        public async Task<Result<HighlightDto>> AddHighlightAsync(string userId, AddHighlightDto dto)
        {
            var highlight = new Highlight
            {
                Content = dto.Content,
                NovelTitle = dto.NovelTitle,
                NovelAuthor = dto.NovelAuthor,
                UserId = userId,
                CreatedAt = EgyptDateTime.Now
            };

            db.Highlights.Add(highlight);
            await db.SaveChangesAsync();

            return Result<HighlightDto>.Success(new HighlightDto(
                highlight.Id,
                highlight.Content,
                highlight.NovelTitle,
                highlight.NovelAuthor,
                highlight.CreatedAt
            ));
        }

        public async Task<Result<string>> DeleteHighlightAsync(string userId, int highlightId)
        {
            var highlight = await db.Highlights
                .FirstOrDefaultAsync(h => h.Id == highlightId && h.UserId == userId);

            if (highlight is null)
                return Result<string>.Failure("الاقتباس غير موجود");

            db.Highlights.Remove(highlight);
            await db.SaveChangesAsync();

            return Result<string>.Success("تم الحذف بنجاح");
        }

        private static ProfileDto MapToDto(AppUser user, List<Sard.Domain.Entities.Novel> novels, List<Highlight> highlights) =>
            new(
                user.Id,
                user.DisplayName,
                user.Bio,
                user.ProfileImageUrl,
                user.CreatedAt,
                novels.Count(n => n.Status == NovelStatus.Published),
                novels.Sum(n => n.ReadCount),
                novels.Select(n => new NovelSummaryDto(
                    n.Id,
                    n.Title,
                    n.Description,
                    n.CoverImageUrl,
                    n.Price,
                    n.Status,
                    n.Chapters.Count,
                    n.LastReadChapterId,
                    n.CreatedAt
                )),
                highlights.Select(h => new HighlightDto(
                    h.Id,
                    h.Content,
                    h.NovelTitle,
                    h.NovelAuthor,
                    h.CreatedAt
                ))
            );
    }
}
