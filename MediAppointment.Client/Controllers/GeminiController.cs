using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MediAppointment.Client.Models.Common;
using System;

namespace MediAppointment.Client.Controllers
{
    [Route("[controller]")]
    public class GeminiController : Controller
    {
        private readonly IGeminiService _geminiService;

        public GeminiController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpGet("Gemini")]
        public IActionResult Index()
        {
            return View("~/Views/Gemini/Gemini.cshtml");
        }

        // FIX: Đổi route để tránh conflict
        [HttpPost("SendMessage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                {
                    Console.WriteLine("Invalid request: Message is null or empty.");
                    return Json(new { success = false, errorMessage = "Tin nhắn không được để trống." });
                }

                var result = await _geminiService.SendChatMessageAsync(request.Message);
                Console.WriteLine($"Chat result: Success={result.Success}, Data={result.Data}, Error={result.ErrorMessage}");

                return Json(new
                {
                    success = result.Success,
                    data = result.Data,
                    errorMessage = result.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GeminiController.Chat: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return Json(new { success = false, errorMessage = $"Lỗi client: {ex.Message}" });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}