namespace Sard.Application.Interfaces.Admin
{
    public interface IAdminService
    {
        Task<Result<AdminStatsDto>> GetStatsAsync();
        Task<Result<IEnumerable<AdminUserDto>>> GetUsersAsync(string? search);
        Task<Result<AdminUserDetailDto>> GetUserDetailAsync(string userId);
        Task<Result<bool>> ToggleLockUserAsync(string userId);
        Task<Result<IEnumerable<AdminPostDto>>> GetPostsAsync();
        Task<Result<string>> DeletePostAsync(int postId);
        Task<Result<string>> DeleteReplyAsync(int replyId);
        Task<Result<IEnumerable<AdminNovelDto>>> GetPublishedNovelsAsync();
        Task<Result<string>> DeleteReplyByAdminAsync(int replyId);
    }
}
