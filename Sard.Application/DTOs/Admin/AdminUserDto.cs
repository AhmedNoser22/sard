namespace Sard.Application.DTOs.Admin
{
    public record AdminUserDto(
    string Id,
    string DisplayName,
    string Email,
    string? ProfileImageUrl,
    string? Bio,
    int FollowersCount,
    int FollowingCount,
    int PublishedNovelsCount,
    int PostsCount,
    bool IsLocked,
    DateTime CreatedAt,
    DateTime? LastActivity
);
}
