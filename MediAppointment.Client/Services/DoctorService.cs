using MediAppointment.Client.Models.Doctor;
using MediAppointment.Client.Models.Common;
using System.Text.Json;
using System.Text;
using MediAppointment.Client.Models.Appointment;

namespace MediAppointment.Client.Services
{
    public interface IDoctorService
    {
        Task<ApiResponse<DoctorViewModel>> GetDoctorProfileAsync(Guid doctorId);
        Task<ApiResponse<DoctorStatusModel>> GetLoggedInDoctorProfileAsync();
        Task<ApiResponse<List<DoctorViewModel>>> GetAllDoctorsAsync();
        Task<ApiResponse<bool>> UpdateDoctorProfileAsync(DoctorUpdateProfile dto);
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

        public async Task<ApiResponse<DoctorViewModel>> GetDoctorProfileAsync(Guid doctorId)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent(JsonSerializer.Serialize(doctorId), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/doctor/profile", content);

                if (response.IsSuccessStatusCode)
                {
                    var contentResponse = await response.Content.ReadAsStringAsync();
                    var doctor = JsonSerializer.Deserialize<DoctorViewModel>(contentResponse, new JsonSerializerOptions
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

        public async Task<ApiResponse<DoctorStatusModel>> GetLoggedInDoctorProfileAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync("/api/doctor/DoctorProfile");

                if (response.IsSuccessStatusCode)
                {
                    var contentResponse = await response.Content.ReadAsStringAsync();
                    var doctor = JsonSerializer.Deserialize<DoctorStatusModel>(contentResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<DoctorStatusModel> { Success = true, Data = doctor };
                }

                return new ApiResponse<DoctorStatusModel> { Success = false, ErrorMessage = "Không thể tải thông tin bác sĩ" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DoctorStatusModel> { Success = false, ErrorMessage = ex.Message };
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

        public async Task<ApiResponse<bool>> UpdateDoctorProfileAsync(DoctorUpdateProfile dto)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("/api/doctor/profile", content);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool> { Success = true };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<bool> { Success = false, ErrorMessage = $"Cập nhật thất bại: {errorContent}" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, ErrorMessage = ex.Message };
            }
        }

        //public async Task<ApiResponse<List<AppointmentViewModel>>> GetAppointmentsAsync(Guid doctorId, DateTime? date = null, DateTime? startDate = null, DateTime? endDate = null)
        //{
        //    try
        //    {
        //        SetAuthHeader();
        //        var queryString = BuildQueryString(doctorId, date, startDate, endDate);
        //        var response = await _httpClient.GetAsync($"/api/doctor/appointments/{doctorId}{queryString}");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var content = await response.Content.ReadAsStringAsync();
        //            var appointments = JsonSerializer.Deserialize<List<AppointmentViewModel>>(content, new JsonSerializerOptions
        //            {
        //                PropertyNameCaseInsensitive = true
        //            });

        //            return new ApiResponse<List<AppointmentViewModel>> { Success = true, Data = appointments ?? new List<AppointmentViewModel>() };
        //        }

        //        return new ApiResponse<List<AppointmentViewModel>> { Success = false, ErrorMessage = "Không thể tải danh sách lịch hẹn" };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiResponse<List<AppointmentViewModel>> { Success = false, ErrorMessage = ex.Message };
        //    }
        //}

        //private string BuildQueryString(Guid doctorId, DateTime? date, DateTime? startDate, DateTime? endDate)
        //{
        //    var queryParams = new List<string>();
        //    if (date.HasValue) queryParams.Add($"date={Uri.EscapeDataString(date.Value.ToString("o"))}");
        //    if (startDate.HasValue) queryParams.Add($"startDate={Uri.EscapeDataString(startDate.Value.ToString("o"))}");
        //    if (endDate.HasValue) queryParams.Add($"endDate={Uri.EscapeDataString(endDate.Value.ToString("o"))}");

        //    return queryParams.Any() ? $"?{string.Join("&", queryParams)}" : "";
        //}
    }
}
