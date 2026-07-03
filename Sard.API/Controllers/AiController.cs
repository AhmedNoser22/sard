using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sard.Application.DTOs.AI;
using Sard.Application.Services;

namespace Sard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiController(IAiService aiService) : ControllerBase
    {
        [HttpPost("correct")]
        public async Task<IActionResult> Correct([FromBody] CorrectTextDto dto)
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
