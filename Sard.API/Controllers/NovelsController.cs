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
        [HttpGet("purchased")]
        [Authorize]
        public async Task<IActionResult> GetPurchasedNovels()
        {
            var result = await novelService.GetPurchasedNovelsAsync(UserId);
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
        [HttpPost("{novelId}/purchase")]
        [Authorize]
        public async Task<IActionResult> PurchaseNovel(int novelId, [FromServices] IPaymentService paymentService)
        {
            var result = await paymentService.InitiateReadPaymentAsync(UserId, novelId);
            return result.IsSuccess ? Ok(new { checkoutUrl = result.Data }) : BadRequest(result.Error);
        }

        [HttpGet("{novelId}/has-purchased")]
        [Authorize]
        public async Task<IActionResult> HasPurchased(int novelId)
        {
            var result = await novelService.HasPurchasedAsync(UserId, novelId);
            return result.IsSuccess ? Ok(new { hasPurchased = result.Data }) : BadRequest(result.Error);
        }

        [HttpGet("{novelId}/download")]
        [Authorize]
        public async Task<IActionResult> DownloadNovel(int novelId)
        {
            var result = await novelService.GetNovelForDownloadAsync(UserId, novelId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        [HttpGet("{novelId}/download/pdf")]
        [Authorize]
        public async Task<IActionResult> DownloadPdf(int novelId, [FromServices] NovelPdfService pdfService)
        {
            var result = await novelService.GetNovelForDownloadAsync(UserId, novelId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            var pdfBytes = pdfService.GeneratePdf(result.Data!);
            var fileName = $"{result.Data!.Title}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        [HttpPost("{novelId}/confirm-purchase")]
        [Authorize]
        public async Task<IActionResult> ConfirmPurchase(int novelId, [FromBody] ConfirmPurchaseDto dto)
        {
            var result = await novelService.ConfirmPurchaseAsync(UserId, novelId, dto.SessionId);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }
    }
}
