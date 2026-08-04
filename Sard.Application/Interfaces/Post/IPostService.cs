namespace Sard.Application.Interfaces.Post
{
    public interface IPostService
    {
        Task<Result<IEnumerable<PostDto>>> GetPostsAsync(string currentUserId, int page, int pageSize);
        Task<Result<PostDto>> CreatePostAsync(string userId, CreatePostDto dto);
        Task<Result<PostDto>> GetPostAsync(int postId, string currentUserId);
        Task<Result<ReplyDto>> AddReplyAsync(string userId, int postId, CreateReplyDto dto);
        Task<Result<(bool IsLiked, int LikesCount)>> ToggleLikeAsync(string userId, int postId);
        Task<Result<string>> DeletePostAsync(string userId, int postId);
        Task<Result<string>> DeleteReplyAsync(string userId, int replyId);
        Task<Result<string>> DeleteReplyByAdminAsync(int replyId);
        Task<Result<string>> SharePostAsync(string userId, int postId);
        Task<Result<IEnumerable<SharedPostDto>>> GetMySharesAsync(string userId);
    }
}
