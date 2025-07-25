using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;

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
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                {
                    Console.WriteLine("Invalid request: Message is null or empty.");
                    return BadRequest(new { message = "Tin nhắn không được để trống." });
                }

                Guid? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out Guid parsedUserId))
                    {
                        userId = parsedUserId;
                        Console.WriteLine($"User authenticated with ID: {userId}");
                    }
                    else
                    {
                        Console.WriteLine("Failed to parse user ID from claims.");
                    }
                }
                else
                {
                    Console.WriteLine("User not authenticated.");
                }

                var response = await _geminiChatService.SendMessageAsync(request.Message, userId, cancellationToken);
                if (string.IsNullOrEmpty(response))
                {
                    Console.WriteLine("Chat service returned empty response.");
                    return Ok(new { reply = "Không nhận được phản hồi từ chatbot." });
                }

                Console.WriteLine($"Chat response: {response}");
                return Ok(new { reply = response });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Chat API: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}