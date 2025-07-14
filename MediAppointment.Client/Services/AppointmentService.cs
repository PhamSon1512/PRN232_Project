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
        
        // New methods for booking
        Task<ApiResponse<List<Models.Appointment.DepartmentOption>>> GetDepartmentsAsync();
        Task<ApiResponse<List<Models.Appointment.TimeSlotOption>>> GetAvailableTimeSlotsAsync(Guid departmentId, DateTime startDate, DateTime endDate);
        Task<ApiResponse<string>> BookAppointmentAsync(Models.Appointment.BookingViewModel model);
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
            // Lấy token từ session và set vào Authorization header
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
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

        public async Task<ApiResponse<List<DepartmentOption>>> GetDepartmentsAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync("/api/appointment/Departments");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var departments = JsonSerializer.Deserialize<List<DepartmentResponseDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var departmentOptions = departments?.Select(d => new Models.Appointment.DepartmentOption
                    {
                        Id = d.Id,
                        Name = d.DepartmentName,
                        TotalRooms = d.TotalRooms
                    }).ToList() ?? new List<Models.Appointment.DepartmentOption>();

                    return new ApiResponse<List<Models.Appointment.DepartmentOption>> { Success = true, Data = departmentOptions };
                }

                return new ApiResponse<List<Models.Appointment.DepartmentOption>> { Success = false, ErrorMessage = "Không thể tải danh sách khoa" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<Models.Appointment.DepartmentOption>> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<List<Models.Appointment.TimeSlotOption>>> GetAvailableTimeSlotsAsync(Guid departmentId, DateTime startDate, DateTime endDate)
        {
            try
            {
                SetAuthHeader();
                
                var request = new
                {
                    DepartmentId = departmentId,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/appointment/GetAvailableTimeSlots", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var timeSlotResponses = JsonSerializer.Deserialize<List<TimeSlotAvailabilityResponseDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var timeSlotOptions = timeSlotResponses?.Select(ts => new Models.Appointment.TimeSlotOption
                        {
                            Id = ts.TimeSlot.Id,
                            TimeRange = ts.TimeSlot.TimeRange,
                            IsAvailable = ts.IsAvailable,
                            AvailableRooms = ts.AvailableRooms,
                            TotalRooms = ts.TotalRooms,
                            Shift = ts.TimeSlot.Shift,
                            Date = ts.Date // QUAN TRỌNG: Set Date từ response
                        }).ToList() ?? new List<Models.Appointment.TimeSlotOption>();

                    return new ApiResponse<List<Models.Appointment.TimeSlotOption>> { Success = true, Data = timeSlotOptions };
                }

                return new ApiResponse<List<Models.Appointment.TimeSlotOption>> { Success = false, ErrorMessage = "Không thể tải thông tin lịch khám" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<Models.Appointment.TimeSlotOption>> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<string>> BookAppointmentAsync(Models.Appointment.BookingViewModel model)
        {
            try
            {
                SetAuthHeader();
                
                var bookRequest = new
                {
                    DepartmentId = model.DepartmentId,
                    TimeSlotId = model.TimeSlotId,
                    AppointmentDate = model.AppointmentDate,
                    Note = model.Note
                };

                var json = JsonSerializer.Serialize(bookRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/appointment/BookAppointment", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<string> { Success = true, Data = "Đặt lịch khám thành công!" };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<string> { Success = false, ErrorMessage = $"Đặt lịch thất bại: {errorContent}" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, ErrorMessage = ex.Message };
            }
        }
    }

    // Supporting DTOs
    public class DepartmentResponseDto
    {
        public Guid Id { get; set; }
        public string DepartmentName { get; set; } = "";
        public int TotalRooms { get; set; }
    }

    public class TimeSlotAvailabilityResponseDto
    {
        public DateTime Date { get; set; }
        public TimeSlotResponseDto TimeSlot { get; set; } = new();
        public bool IsAvailable { get; set; }
        public int AvailableRooms { get; set; }
        public int TotalRooms { get; set; }
    }

    public class TimeSlotResponseDto
    {
        public Guid Id { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Shift { get; set; }
        public string TimeRange => $"{TimeStart:hh\\:mm} - {(TimeStart + Duration):hh\\:mm}";
    }
}
