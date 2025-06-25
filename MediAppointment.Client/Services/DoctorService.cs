using MediAppointment.Client.Models.Doctor;
using MediAppointment.Client.Models.Common;
using System.Text.Json;

namespace MediAppointment.Client.Services
{
    public interface IDoctorService
    {
        Task<ApiResponse<DoctorViewModel>> GetDoctorProfileAsync(Guid doctorId);
        Task<ApiResponse<List<DoctorViewModel>>> GetAllDoctorsAsync();
    }

    public class DoctorService : IDoctorService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DoctorService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthHeader()
        {
            // Với Cookie authentication, không cần set header
            // Cookie sẽ được tự động gửi cùng với request
        }

        public async Task<ApiResponse<DoctorViewModel>> GetDoctorProfileAsync(Guid doctorId)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"/api/doctor/profile/{doctorId}");
                
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
                return new ApiResponse<DoctorViewModel> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<List<DoctorViewModel>>> GetAllDoctorsAsync()
        {
            try
            {
                SetAuthHeader();
                // Note: This endpoint might need to be implemented in the API
                var response = await _httpClient.GetAsync("/api/doctor/all");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var doctors = JsonSerializer.Deserialize<List<DoctorViewModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<List<DoctorViewModel>> { Success = true, Data = doctors ?? new List<DoctorViewModel>() };
                }

                return new ApiResponse<List<DoctorViewModel>> { Success = false, ErrorMessage = "Không thể tải danh sách bác sĩ" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<DoctorViewModel>> { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}
