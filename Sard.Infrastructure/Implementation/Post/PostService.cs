using Sard.Application.Interfaces.Cache;

namespace Sard.Infrastructure.Implementation.Post
{
    public class PostService(
    AppDbContext db,
    UserManager<AppUser> userManager,
    IHubContext<NabdHub> hubContext,
    INotificationService notificationService,
    ICacheService cache) : IPostService
    {
        private const string PostsCacheKey = "posts:all";
        private const string UserPostsPrefix = "posts:user:";
        private const string HighlightsPrefix = "highlights:user:";
        public async Task<Result<IEnumerable<PostDto>>> GetPostsAsync(string currentUserId, int page, int pageSize)
        {
            var cacheKey = $"{PostsCacheKey}:{page}:{pageSize}";
            var cached = await cache.GetAsync<List<PostDto>>(cacheKey);
            if (cached is not null)
                return Result<IEnumerable<PostDto>>.Success(cached);

            var posts = await db.Posts
                .Where(p => p.Status == PostStatus.Active)
                .Include(p => p.User)
                .Include(p => p.Replies).ThenInclude(r => r.User)
                .Include(p => p.Likes).ThenInclude(l => l.User)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = posts.Select(p => MapToDto(p, currentUserId)).ToList();
            await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return Result<IEnumerable<PostDto>>.Success(result);
        }

        public async Task<Result<PostDto>> CreatePostAsync(string userId, CreatePostDto dto)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Result<PostDto>.Failure("المستخدم غير موجود");

            var post = new Sard.Domain.Entities.Post
            {
                Content = dto.Content,
                UserId = userId,
                Status = PostStatus.Active,
                CreatedAt = EgyptDateTime.Now
            };

            db.Posts.Add(post);
            await db.SaveChangesAsync();
            await db.Entry(post).Reference(p => p.User).LoadAsync();

            await cache.RemoveByPrefixAsync(PostsCacheKey);

            var postDto = MapToDto(post, userId);
            await hubContext.Clients.All.SendAsync("NewPost", postDto);

            return Result<PostDto>.Success(postDto);
        }
        public async Task<Result<string>> SharePostAsync(string userId, int postId)
        {
            var already = await db.PostShares.AnyAsync(s => s.UserId == userId && s.PostId == postId);
            if (already)
            {
                var existing = await db.PostShares.FirstAsync(s => s.UserId == userId && s.PostId == postId);
                db.PostShares.Remove(existing);
                await db.SaveChangesAsync();
                await cache.RemoveAsync($"shares:user:{userId}");
                return Result<string>.Success("unshared");
            }

            var post = await db.Posts.FindAsync(postId);
            if (post is null) return Result<string>.Failure("البوست غير موجود");

            db.PostShares.Add(new PostShare
            {
                UserId = userId,
                PostId = postId,
                CreatedAt = EgyptDateTime.Now
            });

            await db.SaveChangesAsync();
            await cache.RemoveAsync($"shares:user:{userId}");
            return Result<string>.Success("shared");
        }

        public async Task<Result<IEnumerable<SharedPostDto>>> GetMySharesAsync(string userId)
        {
            var cacheKey = $"shares:user:{userId}";
            var cached = await cache.GetAsync<List<SharedPostDto>>(cacheKey);
            if (cached is not null)
                return Result<IEnumerable<SharedPostDto>>.Success(cached);

            var shares = await db.PostShares
                .Where(s => s.UserId == userId)
                .Include(s => s.Post).ThenInclude(p => p.User)
                .Include(s => s.Post).ThenInclude(p => p.Likes)
                .Include(s => s.Post).ThenInclude(p => p.Replies)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var result = shares.Select(s => new SharedPostDto(
                s.Id,
                s.PostId,
                s.Post.Content,
                s.Post.User?.DisplayName ?? "",
                s.Post.User?.ProfileImageUrl,
                s.Post.LikesCount,
                s.Post.CommentsCount,
                s.Post.CreatedAt,
                s.CreatedAt
            )).ToList();

            await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
            return Result<IEnumerable<SharedPostDto>>.Success(result);
        }

        public async Task<Result<PostDto>> GetPostAsync(int postId, string currentUserId)
        {
            var post = await db.Posts
                .Include(p => p.User)
                .Include(p => p.Replies).ThenInclude(r => r.User)
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post is null)
                return Result<PostDto>.Failure("البوست غير موجود");

            return Result<PostDto>.Success(MapToDto(post, currentUserId));
        }

        public async Task<Result<ReplyDto>> AddReplyAsync(string userId, int postId, CreateReplyDto dto)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return Result<ReplyDto>.Failure("المستخدم غير موجود");

            var post = await db.Posts.FindAsync(postId);
            if (post is null)
                return Result<ReplyDto>.Failure("البوست غير موجود");

            var reply = new Reply
            {
                Content = dto.Content,
                UserId = userId,
                PostId = postId,
                CreatedAt = EgyptDateTime.Now
            };

            db.Replies.Add(reply);
            post.CommentsCount++;
            await db.SaveChangesAsync();

            var replyDto = new ReplyDto(
                reply.Id,
                reply.Content,
                userId,
                user.DisplayName,
                user.ProfileImageUrl,
                reply.CreatedAt
            );

            await hubContext.Clients
                .Group($"post-{postId}")
                .SendAsync("NewReply", new { postId, reply = replyDto });

            await notificationService.NotifyReplyAsync(userId, post.UserId, postId);

            return Result<ReplyDto>.Success(replyDto);
        }

        public async Task<Result<(bool IsLiked, int LikesCount)>> ToggleLikeAsync(string userId, int postId)
        {
            var post = await db.Posts
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post is null)
                return Result<(bool, int)>.Failure("البوست غير موجود");

            var existing = post.Likes.FirstOrDefault(l => l.UserId == userId);
            bool isLiked;

            if (existing is not null)
            {
                db.PostLikes.Remove(existing);
                post.LikesCount--;
                isLiked = false;
            }
            else
            {
                db.PostLikes.Add(new PostLike
                {
                    UserId = userId,
                    PostId = postId,
                    CreatedAt = EgyptDateTime.Now
                });
                post.LikesCount++;
                isLiked = true;
            }

            await db.SaveChangesAsync();

            await hubContext.Clients
                .Group($"post-{postId}")
                .SendAsync("LikeUpdated", new { postId, likesCount = post.LikesCount });

            if (isLiked)
                await notificationService.NotifyLikeAsync(userId, post.UserId, postId);

            return Result<(bool, int)>.Success((isLiked, post.LikesCount));
        }

        public async Task<Result<string>> DeletePostAsync(string userId, int postId)
        {
            var post = await db.Posts.FindAsync(postId);
            if (post is null)
                return Result<string>.Failure("البوست غير موجود");

            if (post.UserId != userId)
                return Result<string>.Failure("غير مصرح");

            db.Posts.Remove(post);
            await db.SaveChangesAsync();

            await hubContext.Clients.All.SendAsync("PostDeleted", postId);

            return Result<string>.Success("تم الحذف");
        }
        public async Task<Result<string>> DeleteReplyAsync(string userId, int replyId)
        {
            var reply = await db.Replies
                .Include(r => r.Post)
                .FirstOrDefaultAsync(r => r.Id == replyId);

            if (reply is null)
                return Result<string>.Failure("التعليق غير موجود");

            if (reply.UserId != userId && reply.Post.UserId != userId)
                return Result<string>.Failure("غير مصرح");

            var postId = reply.PostId;
            var post = reply.Post;

            try
            {
                db.Replies.Remove(reply);
                post.CommentsCount = Math.Max(0, post.CommentsCount - 1);
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<string>.Success("تم الحذف");
            }

            await hubContext.Clients
                .Group($"post-{postId}")
                .SendAsync("ReplyDeleted", new { postId, replyId });

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
        private static PostDto MapToDto(Domain.Entities.Post p, string currentUserId) => new(
            p.Id,
            p.Content,
            p.UserId,
            p.User?.DisplayName ?? "",
            p.User?.ProfileImageUrl,
            p.LikesCount,
            p.CommentsCount,
            p.Likes?.Any(l => l.UserId == currentUserId) ?? false,
            p.Status,
            p.CreatedAt,
            p.Replies?.OrderBy(r => r.CreatedAt)
            .Select(r => new ReplyDto(
            r.Id, r.Content, r.UserId,
            r.User?.DisplayName ?? "",
            r.User?.ProfileImageUrl,
            r.CreatedAt)) ?? [],
            p.Likes?.Select(l => l.User?.DisplayName ?? "").Where(n => !string.IsNullOrEmpty(n)) ?? []
            );

    }
}
