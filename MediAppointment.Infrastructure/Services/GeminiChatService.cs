using System.Net.Http.Json;
using System.Text.Json;
using MediAppointment.Application.DTOs.GeminiDTOs;
using MediAppointment.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace MediAppointment.Infrastructure.Services
{
    public class GeminiChatService : IGeminiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;

        public GeminiChatService(HttpClient httpClient, IOptions<GeminiSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[] { new { text = message } }
                        }
                    }
                };

                var requestUrl = $"{_settings.Endpoint}?key={_settings.ApiKey}";
                using var response = await _httpClient.PostAsync(requestUrl, JsonContent.Create(requestBody), cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return "Lỗi từ Gemini API.";

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                using var jsonDoc = JsonDocument.Parse(responseContent);

                return jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "Không nhận được phản hồi từ Gemini.";
            }
            catch
            {
                return "Đã xảy ra lỗi khi gọi Gemini API.";
            }
        }
    }


}