[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController(IAdminService adminService, UserManager<AppUser> userManager) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await adminService.GetStatsAsync();
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search)
    {
        var result = await adminService.GetUsersAsync(search);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserDetail(string userId)
    {
        var result = await adminService.GetUserDetailAsync(userId);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("users/{userId}/toggle-lock")]
    public async Task<IActionResult> ToggleLock(string userId)
    {
        var result = await adminService.ToggleLockUserAsync(userId);
        return result.IsSuccess ? Ok(new { isLocked = result.Data }) : BadRequest(result.Error);
    }

    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts()
    {
        var result = await adminService.GetPostsAsync();
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpDelete("posts/{postId}")]
    public async Task<IActionResult> DeletePost(int postId)
    {
        var result = await adminService.DeletePostAsync(postId);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { message = result.Error });
    }

    [HttpDelete("replies/{replyId}")]
    public async Task<IActionResult> DeleteReply(int replyId, [FromServices] IPostService postService)
    {
        var result = await postService.DeleteReplyByAdminAsync(replyId);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { message = result.Error });
    }

    [HttpGet("novels")]
    public async Task<IActionResult> GetNovels()
    {
        var result = await adminService.GetPublishedNovelsAsync();
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }
    [HttpGet("users/{userId}/is-locked")]
    [AllowAnonymous]
    public async Task<IActionResult> IsLocked(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return Ok(new { isLocked = false });
        var isLocked = await userManager.IsLockedOutAsync(user);
        return Ok(new { isLocked });
    }
}