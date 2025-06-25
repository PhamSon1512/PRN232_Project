using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeminiService : ControllerBase
    {
        private readonly IGeminiChatService _geminiChatService;

        public GeminiService(IGeminiChatService geminiChatService)
        {
            _geminiChatService = geminiChatService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Message)) return BadRequest("Message is required.");

            var response = await _geminiChatService.SendMessageAsync(request.Message, cancellationToken);
            return Ok(new { reply = response });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
