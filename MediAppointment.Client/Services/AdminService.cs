using System.Text.Json;
using MediAppointment.Client.Models.Admin;
using MediAppointment.Client.Models.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;

namespace MediAppointment.Client.Services
{
    public interface IAdminService
    {
        Task<ApiResponse<PaginatedResult<AdminViewModel>>> GetAllUsersAsync(int page = 1, int pageSize = 5, string text = "");
        Task<ApiResponse<AdminViewModel>> GetUserByIdAsync(Guid id);
        Task<ApiResponse<bool>> UpgradeToManagerAsync(ManagerCreateDto dto);
        Task<ApiResponse<bool>> UpdateManagerProfileAsync(ManagerUpdateDto dto);
        Task<ApiResponse<AdminViewModel>> GetAdminProfileAsync();
        Task<ApiResponse<bool>> UpdateAdminProfileAsync(AdminUpdateProfile dto);
        Task<ApiResponse<DashboardViewModel>> GetDashboardStatsAsync();
    }

    public class AdminService : IAdminService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine($"Token set from session: {token.Substring(0, 10)}... with full length: {token.Length}");
            }
            else
            {
                Console.WriteLine("No AccessToken found in session.");
            }
        }

        private Guid? GetAdminIdFromSession()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("No AccessToken found in session");
                    return null;
                }

                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;

                Console.WriteLine($"UserId from token: {userIdClaim ?? "null"}");

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var adminId))
                {
                    Console.WriteLine($"Failed to parse userIdClaim: {userIdClaim ?? "null"}");
                    return null;
                }

                return adminId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting admin ID from session: {ex.Message}");
                return null;
            }
        }

        public async Task<ApiResponse<PaginatedResult<AdminViewModel>>> GetAllUsersAsync(int page, int pageSize, string text = "")
        {
            try
            {
                SetAuthHeader();
                var query = $"?text={Uri.EscapeDataString(text ?? "")}&page={page}&pageSize={pageSize}";
                var response = await _httpClient.GetAsync($"/api/Admin/GetAllDoctorsAndManagers{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PaginatedResult<AdminViewModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<PaginatedResult<AdminViewModel>>
                    {
                        Success = true,
                        Data = result ?? new PaginatedResult<AdminViewModel>()
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<PaginatedResult<AdminViewModel>>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi khi lấy danh sách: {errorContent} (Status: {response.StatusCode})"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetAllUsersAsync: {ex.Message}");
                return new ApiResponse<PaginatedResult<AdminViewModel>>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }


        public async Task<ApiResponse<AdminViewModel>> GetUserByIdAsync(Guid id)
        {
            try
            {
                SetAuthHeader();
                var temp = new StringContent(JsonSerializer.Serialize(id), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_configuration["ApiBaseUrl"]}/api/admin/users", temp);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<AdminViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AdminViewModel();
                    return new ApiResponse<AdminViewModel> { Success = true, Data = data };
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<AdminViewModel> { Success = false, ErrorMessage = errorContent };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AdminViewModel> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> UpgradeToManagerAsync(ManagerCreateDto dto)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_configuration["ApiBaseUrl"]}/api/admin/CreateManager", content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool> { Success = true };
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<bool> { Success = false, ErrorMessage = errorContent };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> UpdateManagerProfileAsync(ManagerUpdateDto dto)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(dto), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_configuration["ApiBaseUrl"]}/api/admin/UpdateManagerProfile", content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool> { Success = true };
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<bool> { Success = false, ErrorMessage = errorContent };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<AdminViewModel>> GetAdminProfileAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"{_configuration["ApiBaseUrl"]}/api/admin/AdminProfile");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<AdminViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AdminViewModel();
                    return new ApiResponse<AdminViewModel> { Success = true, Data = data };
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<AdminViewModel> { Success = false, ErrorMessage = errorContent };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AdminViewModel> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> UpdateAdminProfileAsync(AdminUpdateProfile dto)
        {
            try
            {
                SetAuthHeader();

                // Đảm bảo Id được set từ session nếu chưa có
                if (dto.Id == null || dto.Id == Guid.Empty)
                {
                    var adminId = GetAdminIdFromSession();
                    if (adminId == null)
                    {
                        return new ApiResponse<bool> { Success = false, ErrorMessage = "Không thể xác định ID admin từ session." };
                    }
                    dto.Id = adminId.Value;
                }

                // Tạo DTO cho API server
                var serverDto = new
                {
                    AdminId = dto.Id,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber
                };

                var content = new StringContent(JsonSerializer.Serialize(serverDto), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_configuration["ApiBaseUrl"]}/api/admin/UpdateAdminProfile", content);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool> { Success = true };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error: {errorContent}");
                return new ApiResponse<bool> { Success = false, ErrorMessage = errorContent };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateAdminProfileAsync: {ex.Message}");
                return new ApiResponse<bool> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<DashboardViewModel>> GetDashboardStatsAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"{_configuration["ApiBaseUrl"]}/api/admin/dashboard");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<DashboardViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new DashboardViewModel();
                    return new ApiResponse<DashboardViewModel> { Success = true, Data = data };
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<DashboardViewModel>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi khi lấy thống kê dashboard: {errorContent} (Status: {response.StatusCode})"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetDashboardStatsAsync: {ex.Message}");
                return new ApiResponse<DashboardViewModel> { Success = false, ErrorMessage = ex.Message };
            }
        }
    }

    public class ManagerCreateDto
    {
        public Guid DoctorId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ManagerUpdateDto
    {
        public Guid DoctorId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
    }
}