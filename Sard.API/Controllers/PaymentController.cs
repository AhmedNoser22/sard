using Microsoft.AspNetCore.Mvc;
using Sard.Application.Services;

namespace Sard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(IPaymentService paymentService) : ControllerBase
    {
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();

            var result = await paymentService.HandleWebhookAsync(json, signature);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }
    }
}