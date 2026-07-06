using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sard.Application.DTOs.Profile;
using Sard.Application.Interfaces.Profile;
using Sard.Domain.Enums;
using System.Security.Claims;

namespace Sard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles =AppRoles.User)]
    public class ProfileController(IProfileService profileService) : ControllerBase
    {
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var result = await profileService.GetProfileAsync(UserId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var result = await profileService.UpdateProfileAsync(UserId, dto);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpPut("image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateImage([FromForm] UpdateProfileImageDto dto)
        {
            var result = await profileService.UpdateProfileImageAsync(UserId, dto.Image);
            return result.IsSuccess
                ? Ok(new { imageUrl = result.Data })
                : BadRequest(result.Error);
        }

        [HttpPost("highlights")]
        public async Task<IActionResult> AddHighlight([FromBody] AddHighlightDto dto)
        {
            var result = await profileService.AddHighlightAsync(UserId, dto);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpDelete("highlights/{id}")]
        public async Task<IActionResult> DeleteHighlight(int id)
        {
            var result = await profileService.DeleteHighlightAsync(UserId, id);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        [HttpGet("public/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProfile(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var result = await profileService.GetPublicProfileAsync(userId, currentUserId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpPost("follow/{userId}")]
        public async Task<IActionResult> ToggleFollow(string userId)
        {
            var result = await profileService.ToggleFollowAsync(UserId, userId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpPost("favorite-novels")]
        public async Task<IActionResult> AddFavoriteNovel([FromBody] AddFavoriteNovelDto dto)
        {
            var result = await profileService.AddFavoriteNovelAsync(UserId, dto);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpDelete("favorite-novels/{novelId}")]
        public async Task<IActionResult> DeleteFavoriteNovel(int novelId)
        {
            var result = await profileService.DeleteFavoriteNovelAsync(UserId, novelId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        [HttpGet("{userId}/followers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFollowers(string userId)
        {
            var result = await profileService.GetFollowersAsync(userId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpGet("{userId}/following")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFollowing(string userId)
        {
            var result = await profileService.GetFollowingAsync(userId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
    }
}
