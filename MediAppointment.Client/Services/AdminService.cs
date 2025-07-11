using System.Text.Json;
using MediAppointment.Client.Models.Admin;
using MediAppointment.Client.Models.Common;

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
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // Log lỗi nếu token không tồn tại
                Console.WriteLine("No AccessToken found in cookies.");
            }
        }

        public async Task<ApiResponse<PaginatedResult<AdminViewModel>>> GetAllUsersAsync(int page = 1, int pageSize = 5, string text = "")
        {
            try
            {
                SetAuthHeader();
                var url = $"{_configuration["ApiBaseUrl"]}/api/admin/GetAllDoctorsAndManagers?page={page}&pageSize={pageSize}&text={Uri.EscapeDataString(text)}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<PaginatedResult<AdminViewModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new PaginatedResult<AdminViewModel>();

                    return new ApiResponse<PaginatedResult<AdminViewModel>>
                    {
                        Success = true,
                        Data = data
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
                Console.WriteLine($"Exception: {ex.Message}");
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
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(dto), System.Text.Encoding.UTF8, "application/json");
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
                var content = new StringContent(JsonSerializer.Serialize(dto), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_configuration["ApiBaseUrl"]}/api/admin/UpdateAdminProfile", content);
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
    }
    public class ManagerCreateDto
    {
        public Guid DoctorId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class ManagerUpdateDto
    {
        public Guid DoctorId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }
}

