using MediAppointment.Client.Models.Auth;
using MediAppointment.Client.Models.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text;

namespace MediAppointment.Client.Services
{
    public interface IAuthService
    {
        Task<LoginResultDto?> LoginAsync(LoginViewModel model);
        Task<ApiResponse<string>> RegisterAsync(RegisterViewModel model);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
    }    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginResultDto?> LoginAsync(LoginViewModel model)
        {
            try
            {
                var loginDto = new
                {
                    Email = model.Email,
                    Password = model.Password
                };

                var json = JsonSerializer.Serialize(loginDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var loginResult = JsonSerializer.Deserialize<LoginResultDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (loginResult?.Success == true)
                    {
                        // ✅ Giải mã JWT để lấy role
                        var handler = new JwtSecurityTokenHandler();
                        var jwt = handler.ReadJwtToken(loginResult.AccessToken);
                        var role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                        loginResult.Role = role ?? string.Empty;
                        return loginResult;
                    }
                }
            }
            catch
            {
                // Log lỗi nếu cần
            }

            return null;
        }



        public async Task<ApiResponse<string>> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var registerDto = new
                {
                    FullName = model.FullName,
                    Gender = model.Gender,
                    DateOfBirth = model.DateOfBirth,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Password = model.Password,
                    Roles = new List<string> { model.Role }
                };

                var json = JsonSerializer.Serialize(registerDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/register", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<string> { Success = true, Data = "Đăng ký thành công" };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<string> { Success = false, ErrorMessage = errorContent };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, ErrorMessage = ex.Message };
            }
        }        public async Task LogoutAsync()
        {
            _httpContextAccessor.HttpContext?.Session.Remove("UserId");
            _httpContextAccessor.HttpContext?.Session.Remove("UserRole");
            _httpContextAccessor.HttpContext?.Session.Remove("IsAuthenticated");
            await Task.CompletedTask;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var isAuth = _httpContextAccessor.HttpContext?.Session.GetString("IsAuthenticated");
            return await Task.FromResult(isAuth == "true");
        }
    }
}
