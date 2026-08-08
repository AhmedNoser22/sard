namespace Sard.Application.DTOs.Group
{
    public record GroupDto(
        int Id,
        string Name,
        string? Description,
        string? ImageUrl,
        bool IsLocked,
        string CreatorId,
        string CreatorName,
        int MembersCount,
        GroupMemberDto? MyMembership,
        DateTime CreatedAt,
        List<GroupMemberDto> Members
    );
}