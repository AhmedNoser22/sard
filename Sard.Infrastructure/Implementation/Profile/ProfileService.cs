namespace Sard.Infrastructure.Implementation.Profile
{
    public class ProfileService(
    AppDbContext db,
    UserManager<AppUser> userManager,
    IImageService imageService,
    INotificationService notificationService) : IProfileService
    {
        public async Task<Result<ProfileDto>> GetProfileAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Result<ProfileDto>.Failure("المستخدم غير موجود");

            var novels = await db.Novels.Where(n => n.AuthorId == userId).Include(n => n.Chapters).ToListAsync();
            var highlights = await db.Highlights.Where(h => h.UserId == userId).OrderByDescending(h => h.CreatedAt).ToListAsync();
            var favoriteNovels = await db.FavoriteNovels.Where(f => f.UserId == userId).OrderByDescending(f => f.CreatedAt).ToListAsync();
            var followersCount = await db.Follows.CountAsync(f => f.FollowedId == userId);
            var followingCount = await db.Follows.CountAsync(f => f.FollowerId == userId);

            return Result<ProfileDto>.Success(MapToDto(user, novels, highlights, favoriteNovels, followersCount, followingCount));
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

        public async Task<Result<PublicProfileDto>> GetPublicProfileAsync(string targetUserId, string currentUserId)
        {
            var user = await userManager.FindByIdAsync(targetUserId);
            if (user is null)
                return Result<PublicProfileDto>.Failure("المستخدم غير موجود");

            var followersCount = await db.Follows.CountAsync(f => f.FollowedId == targetUserId);
            var followingCount = await db.Follows.CountAsync(f => f.FollowerId == targetUserId);
            var isFollowed = await db.Follows.AnyAsync(f => f.FollowerId == currentUserId && f.FollowedId == targetUserId);

            var highlights = await db.Highlights
                .Where(h => h.UserId == targetUserId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            var favoriteNovels = await db.FavoriteNovels
                .Where(f => f.UserId == targetUserId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return Result<PublicProfileDto>.Success(new PublicProfileDto(
                user.Id,
                user.DisplayName,
                user.Bio,
                user.ProfileImageUrl,
                user.CreatedAt,
                followersCount,
                followingCount,
                isFollowed,
                highlights.Select(h => new HighlightDto(h.Id, h.Content, h.NovelTitle, h.NovelAuthor, h.CreatedAt)),
                favoriteNovels.Select(f => new FavoriteNovelDto(f.Id, f.Title, f.AuthorName, f.CoverImageUrl, 0))
            ));
        }

        public async Task<Result<FollowToggleResultDto>> ToggleFollowAsync(string followerId, string followedId)
        {
            if (followerId == followedId)
                return Result<FollowToggleResultDto>.Failure("لا يمكنك متابعة نفسك");

            var existing = await db.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);

            string action;

            if (existing is not null)
            {
                db.Follows.Remove(existing);
                await db.SaveChangesAsync();
                action = "unfollow";
            }
            else
            {
                try
                {
                    db.Follows.Add(new Follow
                    {
                        FollowerId = followerId,
                        FollowedId = followedId,
                        CreatedAt = EgyptDateTime.Now
                    });
                    await db.SaveChangesAsync();
                    await notificationService.NotifyFollowAsync(followerId, followedId);
                    action = "follow";
                }
                catch (DbUpdateException)
                {
                    action = "follow";
                }
            }
            var followersCount = await db.Follows.CountAsync(f => f.FollowedId == followedId);

            return Result<FollowToggleResultDto>.Success(new FollowToggleResultDto(action, followersCount));
        }
        public async Task<Result<FavoriteNovelDto>> AddFavoriteNovelAsync(string userId, AddFavoriteNovelDto dto)
        {
            var novel = new FavoriteNovel
            {
                Title = dto.Title,
                AuthorName = dto.AuthorName,
                CoverImageUrl = dto.CoverImageUrl,
                UserId = userId,
                CreatedAt = EgyptDateTime.Now
            };

            db.FavoriteNovels.Add(novel);
            await db.SaveChangesAsync();

            return Result<FavoriteNovelDto>.Success(new FavoriteNovelDto(
                novel.Id, novel.Title, novel.AuthorName, novel.CoverImageUrl, 0));
        }

        public async Task<Result<string>> DeleteFavoriteNovelAsync(string userId, int novelId)
        {
            var novel = await db.FavoriteNovels
                .FirstOrDefaultAsync(f => f.Id == novelId && f.UserId == userId);

            if (novel is null)
                return Result<string>.Failure("غير موجود");

            db.FavoriteNovels.Remove(novel);
            await db.SaveChangesAsync();

            return Result<string>.Success("تم الحذف");
        }
        public async Task<Result<IEnumerable<FollowUserDto>>> GetFollowersAsync(string userId)
        {
            var followers = await db.Follows
                .Where(f => f.FollowedId == userId)
                .Include(f => f.Follower)
                .Select(f => new FollowUserDto(f.Follower.Id, f.Follower.DisplayName, f.Follower.ProfileImageUrl))
                .ToListAsync();

            return Result<IEnumerable<FollowUserDto>>.Success(followers);
        }

        public async Task<Result<IEnumerable<FollowUserDto>>> GetFollowingAsync(string userId)
        {
            var following = await db.Follows
                .Where(f => f.FollowerId == userId)
                .Include(f => f.Followed)
                .Select(f => new FollowUserDto(f.Followed.Id, f.Followed.DisplayName, f.Followed.ProfileImageUrl))
                .ToListAsync();

            return Result<IEnumerable<FollowUserDto>>.Success(following);
        }
        private static ProfileDto MapToDto(AppUser user, List<Sard.Domain.Entities.Novel> novels, List<Highlight> highlights, List<FavoriteNovel> favoriteNovels, int followersCount, int followingCount) =>
            new(
                user.Id,
                user.DisplayName,
                user.Bio,
                user.ProfileImageUrl,
                user.CreatedAt,
                novels.Count(n => n.Status == NovelStatus.Published),
                novels.Sum(n => n.ReadCount),
                followersCount,
                followingCount,
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
                )),
                 favoriteNovels.Select(f => new FavoriteNovelDto(f.Id, f.Title, f.AuthorName, f.CoverImageUrl, 0))
            );
    }
}
