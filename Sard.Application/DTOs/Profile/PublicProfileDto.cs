namespace Sard.Application.DTOs.Profile
{
    public record PublicProfileDto(
    string Id,
    string DisplayName,
    string? Bio,
    string? ProfileImageUrl,
    int FollowersCount,
    int FollowingCount,
    bool IsFollowedByMe,
    IEnumerable<HighlightDto> Highlights,
    IEnumerable<FavoriteNovelDto> FavoriteNovels
);
}
