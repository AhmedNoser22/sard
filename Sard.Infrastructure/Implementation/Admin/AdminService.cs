namespace Sard.Infrastructure.Implementation.Admin
{
    public class AdminService(
    AppDbContext db,
    IHubContext<NabdHub> hubContext,
    UserManager<AppUser> userManager) : IAdminService
    {
        public async Task<Result<AdminStatsDto>> GetStatsAsync()
        {
            var totalUsers = await userManager.Users.CountAsync();
            var totalPosts = await db.Posts.CountAsync();
            var totalNovels = await db.Novels.CountAsync(n => n.Status == NovelStatus.Published);
            var purchases = await db.Purchases.Where(p => p.Type == PurchaseType.PublishFee).ToListAsync();

            return Result<AdminStatsDto>.Success(new AdminStatsDto(
                totalUsers, totalPosts, totalNovels,
                purchases.Count,
                purchases.Sum(p => p.Amount)
            ));
        }

        public async Task<Result<IEnumerable<AdminUserDto>>> GetUsersAsync(string? search)
        {
            var query = userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u =>
                    u.DisplayName.Contains(search) ||
                    u.Email!.Contains(search));

            var users = await query.ToListAsync();

            var result = new List<AdminUserDto>();
            foreach (var u in users)
            {
                var followersCount = await db.Follows.CountAsync(f => f.FollowedId == u.Id);
                var followingCount = await db.Follows.CountAsync(f => f.FollowerId == u.Id);
                var novelsCount = await db.Novels.CountAsync(n => n.AuthorId == u.Id && n.Status == NovelStatus.Published);
                var postsCount = await db.Posts.CountAsync(p => p.UserId == u.Id);
                var isLocked = await userManager.IsLockedOutAsync(u);

                result.Add(new AdminUserDto(
                    u.Id, u.DisplayName, u.Email!, u.ProfileImageUrl, u.Bio,
                    followersCount, followingCount, novelsCount, postsCount,
                    isLocked, u.CreatedAt, null
                ));
            }

            return Result<IEnumerable<AdminUserDto>>.Success(result);
        }

        public async Task<Result<AdminUserDetailDto>> GetUserDetailAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return Result<AdminUserDetailDto>.Failure("المستخدم غير موجود");

            var followers = await db.Follows
                .Where(f => f.FollowedId == userId)
                .Include(f => f.Follower)
                .Select(f => new FollowUserDto(f.Follower.Id, f.Follower.DisplayName, f.Follower.ProfileImageUrl))
                .ToListAsync();

            var following = await db.Follows
                .Where(f => f.FollowerId == userId)
                .Include(f => f.Followed)
                .Select(f => new FollowUserDto(f.Followed.Id, f.Followed.DisplayName, f.Followed.ProfileImageUrl))
                .ToListAsync();

            var novels = await db.Novels
                .Where(n => n.AuthorId == userId && n.Status == NovelStatus.Published)
                .Include(n => n.Chapters)
                .Select(n => new AdminNovelDto(
                    n.Id, n.Title, user.DisplayName, n.CoverImageUrl,
                    n.Price, n.Chapters.Count, n.ReadCount, n.CreatedAt))
                .ToListAsync();

            var posts = await db.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.Likes)
                .Include(p => p.Replies)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new AdminPostDto(
                    p.Id,
                    p.Content,
                    user.Id,
                    user.DisplayName,
                    user.ProfileImageUrl,
                    p.LikesCount,
                    p.CommentsCount,
                    p.Status,
                    p.CreatedAt,
                    Enumerable.Empty<AdminReplyDto>(),
                    Enumerable.Empty<string>()))
                .ToListAsync();

            var isLocked = await userManager.IsLockedOutAsync(user);

            return Result<AdminUserDetailDto>.Success(new AdminUserDetailDto(
                user.Id, user.DisplayName, user.Email!, user.ProfileImageUrl, user.Bio,
                isLocked, user.CreatedAt,
                followers.Count, following.Count, novels.Count, posts.Count,
                followers, following, novels, posts
            ));
        }

        public async Task<Result<bool>> ToggleLockUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return Result<bool>.Failure("المستخدم غير موجود");

            var isLocked = await userManager.IsLockedOutAsync(user);

            if (isLocked)
            {
                await userManager.SetLockoutEndDateAsync(user, null);
                return Result<bool>.Success(false);
            }
            else
            {
                await userManager.SetLockoutEnabledAsync(user, true);
                await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                return Result<bool>.Success(true);
            }
        }

        public async Task<Result<IEnumerable<AdminPostDto>>> GetPostsAsync()
        {
            var posts = await db.Posts
                .Include(p => p.User)
                .Include(p => p.Replies).ThenInclude(r => r.User)
                .Include(p => p.Likes).ThenInclude(l => l.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Result<IEnumerable<AdminPostDto>>.Success(posts.Select(p => new AdminPostDto(
                p.Id,
                p.Content,
                p.UserId,
                p.User?.DisplayName ?? "",
                p.User?.ProfileImageUrl,
                p.LikesCount,
                p.CommentsCount,
                p.Status,
                p.CreatedAt,
                p.Replies?.OrderBy(r => r.CreatedAt).Select(r => new AdminReplyDto(
                    r.Id, r.Content, r.UserId,
                    r.User?.DisplayName ?? "",
                    r.User?.ProfileImageUrl,
                    r.CreatedAt)) ?? [],
                p.Likes?.Select(l => l.User?.DisplayName ?? "").Where(n => !string.IsNullOrEmpty(n)) ?? []
            )));
        }

        public async Task<Result<string>> DeletePostAsync(int postId)
        {
            var post = await db.Posts.FindAsync(postId);
            if (post is null)
                return Result<string>.Failure("البوست غير موجود");

            db.Posts.Remove(post);
            await db.SaveChangesAsync();
            return Result<string>.Success("تم الحذف");
        }

        public async Task<Result<string>> DeleteReplyAsync(int replyId)
        {
            var reply = await db.Replies.FindAsync(replyId);
            if (reply is null)
                return Result<string>.Failure("التعليق غير موجود");

            db.Replies.Remove(reply);
            await db.SaveChangesAsync();
            return Result<string>.Success("تم الحذف");
        }

        public async Task<Result<string>> DeleteReplyByAdminAsync(int replyId)
        {
            var reply = await db.Replies
                .Include(r => r.Post)
                .FirstOrDefaultAsync(r => r.Id == replyId);

            if (reply is null)
                return Result<string>.Failure("التعليق غير موجود");

            var post = reply.Post;
            db.Replies.Remove(reply);
            post.CommentsCount = Math.Max(0, post.CommentsCount - 1);
            await db.SaveChangesAsync();

            await hubContext.Clients
                .Group($"post-{reply.PostId}")
                .SendAsync("ReplyDeleted", new { postId = reply.PostId, replyId });

            return Result<string>.Success("تم الحذف");
        }
        public async Task<Result<IEnumerable<AdminNovelDto>>> GetPublishedNovelsAsync()
        {
            var novels = await db.Novels
                .Where(n => n.Status == NovelStatus.Published)
                .Include(n => n.Author)
                .Include(n => n.Chapters)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new AdminNovelDto(
                    n.Id, n.Title, n.Author.DisplayName, n.CoverImageUrl,
                    n.Price, n.Chapters.Count, n.ReadCount, n.CreatedAt))
                .ToListAsync();

            return Result<IEnumerable<AdminNovelDto>>.Success(novels);
        }
    }
}
