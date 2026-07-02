namespace Sard.Application.Interfaces.Profile
{
    public interface IProfileService
    {
        Task<Result<ProfileDto>> GetProfileAsync(string userId);
        Task<Result<ProfileDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<Result<string>> UpdateProfileImageAsync(string userId, IFormFile image);
        Task<Result<HighlightDto>> AddHighlightAsync(string userId, AddHighlightDto dto);
        Task<Result<string>> DeleteHighlightAsync(string userId, int highlightId);
    }
}
