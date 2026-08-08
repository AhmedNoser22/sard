namespace Sard.Application.DTOs.Group
{
    public record GroupMemberDto(
        int Id,
        string UserId,
        string DisplayName,
        string? ProfileImageUrl,
        string Role,
        DateTime JoinedAt
    );
}