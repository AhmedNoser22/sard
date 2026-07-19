using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sard.Application.DTOs.Novel;
using Sard.Application.Interfaces.Novel;
using Sard.Application.Services;
using Sard.Domain.Enums;
using System.Security.Claims;

namespace Sard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.User)]
    public class NovelsController(INovelService novelService) : ControllerBase
    {
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost]
        public async Task<IActionResult> CreateNovel([FromBody] CreateNovelDto dto)
        {
            var result = await novelService.CreateNovelAsync(UserId, dto);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        [HttpPost("{novelId}/cover")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCover(int novelId, [FromForm] UploadCoverDto dto)
        {
            var result = await novelService.UploadCoverAsync(UserId, novelId, dto.Cover);
            return result.IsSuccess ? Ok(new { coverImageUrl = result.Data }) : BadRequest(result.Error);
        }

        [HttpPut("{novelId}")]
        public async Task<IActionResult> UpdateNovel(int novelId, [FromBody] UpdateNovelDto dto)
        {
            var result = await novelService.UpdateNovelAsync(UserId, novelId, dto);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpGet("{novelId}/chapters")]
        public async Task<IActionResult> GetChapters(int novelId)
        {
            var result = await novelService.GetChaptersAsync(UserId, novelId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpGet("{novelId}/chapters/{chapterId}")]
        public async Task<IActionResult> GetChapter(int novelId, int chapterId)
        {
            var result = await novelService.GetChapterAsync(UserId, novelId, chapterId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpPost("{novelId}/chapters")]
        public async Task<IActionResult> SaveChapter(int novelId, [FromBody] SaveChapterDto dto, [FromQuery] int? chapterId)
        {
            var result = await novelService.SaveChapterAsync(UserId, novelId, dto, chapterId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpPut("{novelId}/last-chapter/{chapterId}")]
        public async Task<IActionResult> SetLastChapter(int novelId, int chapterId)
        {
            var result = await novelService.SetLastReadChapterAsync(UserId, novelId, chapterId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        [HttpPut("{novelId}/settings")]
        public async Task<IActionResult> UpdateSettings(int novelId, [FromBody] UpdateNovelSettingsDto dto)
        {
            var result = await novelService.UpdateNovelSettingsAsync(UserId, novelId, dto);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpGet("published")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublished([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await novelService.GetPublishedNovelsAsync(page, pageSize);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        [HttpPost("{novelId}/initiate-publish")]
        public async Task<IActionResult> InitiatePublish(int novelId, [FromServices] IPaymentService paymentService)
        {
            var result = await paymentService.InitiatePublishPaymentAsync(UserId, novelId);
            return result.IsSuccess ? Ok(new { iframeUrl = result.Data }) : BadRequest(result.Error);
        }
    }
}
