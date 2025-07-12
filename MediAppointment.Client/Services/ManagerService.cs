using System.Security.Claims;
using System.Text;
using System.Text.Json;
using MediAppointment.Client.Models.Common;
using MediAppointment.Client.Models.Doctor;
using MediAppointment.Client.Models.Manager;

namespace MediAppointment.Client.Services
{
    public interface IManagerService
    {
        Task<ApiResponse<PaginatedResult<DoctorViewModel>>> GetAllDoctorsAsync(string text, /*string department,*/ int page, int pageSize);
        Task<ApiResponse<DoctorViewModel>> GetDoctorByIdAsync(Guid doctorId);
        Task<ApiResponse<DoctorViewModel>> UpdateDoctorProfileAsync(Guid doctorId, DoctorUpdateModel dto);
        Task<ApiResponse<Guid>> CreateDoctorAsync(DoctorCreateModel dto);
        Task<ApiResponse<bool>> DeleteDoctorAsync(Guid doctorId);
        Task<ApiResponse<ManagerViewModel>> GetManagerProfileAsync();
        Task<ApiResponse<bool>> UpdateManagerProfileAsync(ManagerUpdateProfile dto);
    }

    public class ManagerService : IManagerService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ManagerService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
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
                Console.WriteLine("No AccessToken found in cookies.");
            }
        }

        public async Task<ApiResponse<PaginatedResult<DoctorViewModel>>> GetAllDoctorsAsync(string text, /*string department,*/ int page, int pageSize)
        {
            try
            {
                SetAuthHeader();
                var query = $"?text={Uri.EscapeDataString(text ?? "")}&page={page}&pageSize={pageSize}";
                var response = await _httpClient.GetAsync($"/api/Manager/GetAllDoctors{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PaginatedResult<DoctorViewModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<PaginatedResult<DoctorViewModel>> { Success = true, Data = result };
                }

                return new ApiResponse<PaginatedResult<DoctorViewModel>> { Success = false, ErrorMessage = "Không thể tải danh sách bác sĩ" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetAllDoctorsAsync: {ex.Message}");
                return new ApiResponse<PaginatedResult<DoctorViewModel>> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<DoctorViewModel>> GetDoctorByIdAsync(Guid doctorId)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"/api/Manager/doctors/{doctorId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var doctor = JsonSerializer.Deserialize<DoctorViewModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<DoctorViewModel> { Success = true, Data = doctor };
                }

                return new ApiResponse<DoctorViewModel> { Success = false, ErrorMessage = "Không thể tải thông tin bác sĩ" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetDoctorByIdAsync: {ex.Message}");
                return new ApiResponse<DoctorViewModel> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<DoctorViewModel>> UpdateDoctorProfileAsync(Guid doctorId, DoctorUpdateModel dto)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/api/Manager/doctors/{doctorId}", content);

                if (response.IsSuccessStatusCode)
                {
                    var updatedDoctor = JsonSerializer.Deserialize<DoctorViewModel>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return new ApiResponse<DoctorViewModel> { Success = true, Data = updatedDoctor };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                return new ApiResponse<DoctorViewModel>
                {
                    Success = false,
                    ErrorMessage = errorObj?.GetValueOrDefault("Message") ?? $"Cập nhật hồ sơ bác sĩ thất bại: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateDoctorProfileAsync: {ex.Message}");
                return new ApiResponse<DoctorViewModel> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<Guid>> CreateDoctorAsync(DoctorCreateModel dto)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Manager/CreateDoctor", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
                    if (Guid.TryParse(result?.GetValueOrDefault("DoctorId"), out var doctorId))
                    {
                        return new ApiResponse<Guid> { Success = true, Data = doctorId };
                    }
                    return new ApiResponse<Guid> { Success = false, ErrorMessage = "Invalid DoctorId returned from API" };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                return new ApiResponse<Guid>
                {
                    Success = false,
                    ErrorMessage = errorObj?.GetValueOrDefault("Message") ?? $"Tạo bác sĩ thất bại: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateDoctorAsync: {ex.Message}");
                return new ApiResponse<Guid> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> DeleteDoctorAsync(Guid doctorId)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.DeleteAsync($"/api/Manager/doctors/{doctorId}");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool> { Success = true };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                return new ApiResponse<bool>
                {
                    Success = false,
                    ErrorMessage = errorObj?.GetValueOrDefault("Message") ?? $"Xóa bác sĩ thất bại: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DeleteDoctorAsync: {ex.Message}");
                return new ApiResponse<bool> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<ManagerViewModel>> GetManagerProfileAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync("/api/Manager/profile");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var profile = JsonSerializer.Deserialize<ManagerViewModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<ManagerViewModel> { Success = true, Data = profile };
                }

                return new ApiResponse<ManagerViewModel> { Success = false, ErrorMessage = "Không thể tải thông tin hồ sơ Manager" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetManagerProfileAsync: {ex.Message}");
                return new ApiResponse<ManagerViewModel> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> UpdateManagerProfileAsync(ManagerUpdateProfile dto)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("/api/Manager/profile", content);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool> { Success = true };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                return new ApiResponse<bool>
                {
                    Success = false,
                    ErrorMessage = errorObj?.GetValueOrDefault("Message") ?? $"Cập nhật hồ sơ thất bại: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateManagerProfileAsync: {ex.Message}");
                return new ApiResponse<bool> { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}