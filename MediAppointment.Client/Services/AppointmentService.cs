using MediAppointment.Client.Models.Appointment;
using MediAppointment.Client.Models.Common;
using System.Text.Json;
using System.Text;

namespace MediAppointment.Client.Services
{
    public interface IAppointmentService
    {
        Task<ApiResponse<List<AppointmentViewModel>>> GetMyAppointmentsAsync();
        Task<ApiResponse<AppointmentViewModel>> GetAppointmentDetailAsync(Guid appointmentId);
        Task<ApiResponse<string>> CreateAppointmentAsync(CreateAppointmentViewModel model);
        Task<ApiResponse<string>> CancelAppointmentAsync(Guid appointmentId);
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;        public AppointmentService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }        private void SetAuthHeader()
        {
            // Với Cookie authentication, không cần set header
            // Cookie sẽ được tự động gửi cùng với request
        }

        public async Task<ApiResponse<List<AppointmentViewModel>>> GetMyAppointmentsAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync("/api/appointment/MyAppointment");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var appointments = JsonSerializer.Deserialize<List<AppointmentViewModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<List<AppointmentViewModel>> { Success = true, Data = appointments ?? new List<AppointmentViewModel>() };
                }

                return new ApiResponse<List<AppointmentViewModel>> { Success = false, ErrorMessage = "Không thể tải danh sách cuộc hẹn" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<AppointmentViewModel>> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<AppointmentViewModel>> GetAppointmentDetailAsync(Guid appointmentId)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"/api/appointment/MyAppointment/{appointmentId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var appointment = JsonSerializer.Deserialize<AppointmentViewModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<AppointmentViewModel> { Success = true, Data = appointment };
                }

                return new ApiResponse<AppointmentViewModel> { Success = false, ErrorMessage = "Không thể tải chi tiết cuộc hẹn" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AppointmentViewModel> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<string>> CreateAppointmentAsync(CreateAppointmentViewModel model)
        {
            try
            {
                SetAuthHeader();
                
                var createRequest = new
                {
                    AppointmentDate = model.AppointmentDate,
                    Note = model.Note,
                    RoomTimeSlotId = model.RoomTimeSlotId
                };

                var json = JsonSerializer.Serialize(createRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/appointment", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<string> { Success = true, Data = "Đặt lịch hẹn thành công" };
                }

                return new ApiResponse<string> { Success = false, ErrorMessage = "Đặt lịch hẹn thất bại" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<string>> CancelAppointmentAsync(Guid appointmentId)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"/api/appointment/CancelAppoint/{appointmentId}");
                
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<string> { Success = true, Data = "Hủy lịch hẹn thành công" };
                }

                return new ApiResponse<string> { Success = false, ErrorMessage = "Hủy lịch hẹn thất bại" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}
