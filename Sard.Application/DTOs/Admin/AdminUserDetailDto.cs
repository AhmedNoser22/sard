namespace Sard.Application.DTOs.Admin
{
    public record AdminUserDetailDto(
    string Id,
    string DisplayName,
    string Email,
    string? ProfileImageUrl,
    string? Bio,
    bool IsLocked,
    DateTime CreatedAt,
    int FollowersCount,
    int FollowingCount,
    int PublishedNovelsCount,
    int PostsCount,
    IEnumerable<FollowUserDto> Followers,
    IEnumerable<FollowUserDto> Following,
    IEnumerable<AdminNovelDto> PublishedNovels,
    IEnumerable<AdminPostDto> RecentPosts
);
}
