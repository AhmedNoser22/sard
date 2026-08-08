namespace Sard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.User)]
    public class NabdController(IPostService postService) : ControllerBase
    {
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await postService.GetPostsAsync(UserId, page, pageSize);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            var result = await postService.CreatePostAsync(UserId, dto);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPost(int postId)
        {
            var result = await postService.GetPostAsync(postId, UserId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        [HttpPost("{postId}/share")]
        public async Task<IActionResult> SharePost(int postId)
        {
            var result = await postService.SharePostAsync(UserId, postId);
            return result.IsSuccess ? Ok(new { action = result.Data }) : BadRequest(result.Error);
        }

        [HttpGet("my-shares")]
        public async Task<IActionResult> GetMyShares()
        {
            var result = await postService.GetMySharesAsync(UserId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpPost("{postId}/reply")]
        public async Task<IActionResult> AddReply(int postId, [FromBody] CreateReplyDto dto)
        {
            var result = await postService.AddReplyAsync(UserId, postId, dto);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpPost("{postId}/like")]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var result = await postService.ToggleLikeAsync(UserId, postId);
            return result.IsSuccess ? Ok(new { isLiked = result.Data.IsLiked, likesCount = result.Data.LikesCount }) : BadRequest(result.Error);
        }

        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var result = await postService.DeletePostAsync(UserId, postId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        [HttpDelete("{postId}/reply/{replyId}")]
        public async Task<IActionResult> DeleteReply(int postId, int replyId)
        {
            var result = await postService.DeleteReplyAsync(UserId, replyId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
    }
}
