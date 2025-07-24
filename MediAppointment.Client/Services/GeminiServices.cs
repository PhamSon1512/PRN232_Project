using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MediAppointment.Client.Models.Common;
using Microsoft.Extensions.Configuration;

namespace MediAppointment.Client.Services
{
    public interface IGeminiService
    {
        Task<ApiResponse<string>> SendChatMessageAsync(string message);
    }

    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public GeminiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        private void SetAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine($"Token set from session: {token.Substring(0, Math.Min(10, token.Length))}... with full length: {token.Length}");
            }
            else
            {
                Console.WriteLine("No AccessToken found in session.");
            }
        }

        public async Task<ApiResponse<string>> SendChatMessageAsync(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Tin nhắn không được để trống."
                    };
                }

                SetAuthHeader();
                var request = new { Message = message };

                // FIX: Sử dụng absolute URL từ configuration hoặc hardcode
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7230";
                var apiUrl = $"{apiBaseUrl.TrimEnd('/')}/api/GeminiService/chat";

                Console.WriteLine($"Calling API URL: {apiUrl}");
                var response = await _httpClient.PostAsJsonAsync(apiUrl, request);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: Status {response.StatusCode}, Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(responseContent))
                    {
                        Console.WriteLine("API returned empty response content.");
                        return new ApiResponse<string>
                        {
                            Success = false,
                            ErrorMessage = "Phản hồi từ API rỗng."
                        };
                    }

                    try
                    {
                        var result = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        var reply = result.GetProperty("reply").GetString();
                        return new ApiResponse<string>
                        {
                            Success = true,
                            Data = reply ?? "Không nhận được phản hồi từ chatbot."
                        };
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Failed to deserialize response content as JSON: {ex.Message}, Content: {responseContent}");
                        return new ApiResponse<string>
                        {
                            Success = false,
                            ErrorMessage = $"Phản hồi từ API không phải JSON hợp lệ: {responseContent}"
                        };
                    }
                }

                Dictionary<string, string>? errorObj = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Failed to deserialize error content as JSON: {ex.Message}, Content: {responseContent}");
                    return new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = $"Gửi tin nhắn thất bại: {responseContent}"
                    };
                }

                return new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = errorObj?.GetValueOrDefault("message") ?? $"Gửi tin nhắn thất bại: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SendChatMessageAsync: {ex.Message}");
                return new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}