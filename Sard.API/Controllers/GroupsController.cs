[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController(IGroupService groupService, IImageService imageService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetMyGroups()
    {
        var result = await groupService.GetMyGroupsAsync(UserId);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet("{groupId}")]
    public async Task<IActionResult> GetGroup(int groupId)
    {
        var result = await groupService.GetGroupAsync(groupId, UserId);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        var result = await groupService.CreateGroupAsync(UserId, dto);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("{groupId}/image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadGroupImage(
    int groupId,
    [FromForm] UploadGroupImageDto dto)
    {
        var upload = await imageService.UploadAsync(dto.Image, "sard/groups");

        if (!upload.IsSuccess)
            return BadRequest(upload.Error);

        var group = await groupService.GetGroupAsync(groupId, UserId);

        if (!group.IsSuccess)
            return BadRequest(group.Error);

        return Ok(new { imageUrl = upload.Data });
    }

    [HttpPost("{groupId}/members/{targetUserId}")]
    public async Task<IActionResult> AddMember(int groupId, string targetUserId)
    {
        var result = await groupService.AddMemberAsync(UserId, groupId, targetUserId);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpDelete("{groupId}/members/{targetUserId}")]
    public async Task<IActionResult> RemoveMember(int groupId, string targetUserId)
    {
        var result = await groupService.RemoveMemberAsync(UserId, groupId, targetUserId);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("{groupId}/members/{targetUserId}/promote")]
    public async Task<IActionResult> PromoteToAdmin(int groupId, string targetUserId)
    {
        var result = await groupService.PromoteToAdminAsync(UserId, groupId, targetUserId);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("{groupId}/toggle-lock")]
    public async Task<IActionResult> ToggleLock(int groupId)
    {
        var result = await groupService.ToggleLockAsync(UserId, groupId);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet("{groupId}/mutuals")]
    public async Task<IActionResult> GetMutuals(int groupId)
    {
        var result = await groupService.GetMutualsAsync(UserId, groupId);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet("{groupId}/messages")]
    public async Task<IActionResult> GetMessages(int groupId, [FromQuery] int page = 1)
    {
        var result = await groupService.GetMessagesAsync(UserId, groupId, page);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("{groupId}/messages")]
    public async Task<IActionResult> SendMessage(int groupId, [FromBody] SendMessageDto dto)
    {
        var result = await groupService.SendMessageAsync(UserId, groupId, dto);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("{groupId}/messages/{messageId}/react")]
    public async Task<IActionResult> ReactToMessage(int groupId, int messageId, [FromBody] ReactToMessageDto dto)
    {
        var result = await groupService.ReactToMessageAsync(UserId, groupId, messageId, dto.Emoji);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpDelete("{groupId}/messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage(int groupId, int messageId)
    {
        var result = await groupService.DeleteMessageAsync(UserId, groupId, messageId);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }
    [HttpPost("{groupId}/members/{targetUserId}/demote")]
    public async Task<IActionResult> DemoteFromAdmin(int groupId, string targetUserId)
    {
        var result = await groupService.DemoteFromAdminAsync(UserId, groupId, targetUserId);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }
}