namespace Sard.Application.Interfaces.Group
{
    public interface IGroupService
    {
        Task<Result<IEnumerable<GroupDto>>> GetMyGroupsAsync(string userId);
        Task<Result<GroupDto>> GetGroupAsync(int groupId, string userId);
        Task<Result<GroupDto>> CreateGroupAsync(string userId, CreateGroupDto dto);
        Task<Result<GroupDto>> AddMemberAsync(string requesterId, int groupId, string targetUserId);
        Task<Result<GroupDto>> RemoveMemberAsync(string requesterId, int groupId, string targetUserId);
        Task<Result<GroupDto>> PromoteToAdminAsync(string requesterId, int groupId, string targetUserId);
        Task<Result<GroupDto>> ToggleLockAsync(string requesterId, int groupId);
        Task<Result<IEnumerable<GroupMemberDto>>> GetMutualsAsync(string userId, int groupId);
        Task<Result<IEnumerable<GroupMessageDto>>> GetMessagesAsync(string userId, int groupId, int page);
        Task<Result<GroupMessageDto>> SendMessageAsync(string userId, int groupId, SendMessageDto dto);
        Task<Result<GroupMessageDto>> ReactToMessageAsync(string userId, int groupId, int messageId, string emoji);
        Task<Result<GroupMessageDto>> DeleteMessageAsync(string userId, int groupId, int messageId);
        Task<Result<GroupDto>> DemoteFromAdminAsync(string requesterId, int groupId, string targetUserId);
    }
}