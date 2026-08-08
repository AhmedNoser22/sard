namespace Sard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.User)]
    public class AiController(IAiService aiService) : ControllerBase
    {
        [HttpPost("correct")]
        public async Task<IActionResult> Correct([FromBody] Sard.Application.DTOs.AI.CorrectTextDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest("لا يوجد نص للتصحيح");

            var result = await aiService.CorrectTextAsync(dto.Text);

            return result.IsSuccess
                ? Ok(new CorrectedTextDto(result.Data!))
                : BadRequest(result.Error);
        }
    }
}
