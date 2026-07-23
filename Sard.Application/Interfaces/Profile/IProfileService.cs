namespace Sard.Application.Interfaces.Profile
{
    public interface IProfileService
    {
        Task<Result<ProfileDto>> GetProfileAsync(string userId);
        Task<Result<ProfileDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<Result<string>> UpdateProfileImageAsync(string userId, IFormFile image);
        Task<Result<HighlightDto>> AddHighlightAsync(string userId, AddHighlightDto dto);
        Task<Result<string>> DeleteHighlightAsync(string userId, int highlightId);
        Task<Result<PublicProfileDto>> GetPublicProfileAsync(string targetUserId, string currentUserId);
        Task<Result<FollowToggleResultDto>> ToggleFollowAsync(string followerId, string followedId);
        //Task<Result<FavoriteNovelDto>> AddFavoriteNovelAsync(string userId, AddFavoriteNovelDto dto);
        Task<Result<FavoriteNovelDto>> AddFavoriteNovelAsync(string userId, AddFavoriteNovelDto dto, IFormFile? cover);
        Task<Result<string>> DeleteFavoriteNovelAsync(string userId, int novelId);
        Task<Result<IEnumerable<FollowUserDto>>> GetFollowersAsync(string userId);
        Task<Result<IEnumerable<FollowUserDto>>> GetFollowingAsync(string userId);
    }
}
