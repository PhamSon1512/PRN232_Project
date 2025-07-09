using MediAppointment.Client.Models.Appointment;
using System.Text.Json;
using System.Text;

namespace MediAppointment.Client.Services
{
    public interface ITimeSlotService
    {
        Task<ServiceResult<List<TimeSlotOption>>> GetAvailableTimeSlotsAsync(Guid departmentId, DateTime startDate, DateTime endDate);
        Task<ServiceResult<List<TimeSlotOption>>> CheckTimeSlotAvailabilityAsync(Guid departmentId, DateTime date);
        Task<ServiceResult<List<TimeSlotOption>>> GetTimeSlotsAsync();
    }

    public class TimeSlotService : ITimeSlotService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TimeSlotService> _logger;

        public TimeSlotService(IHttpClientFactory httpClientFactory, ILogger<TimeSlotService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
        }

        public async Task<ServiceResult<List<TimeSlotOption>>> GetAvailableTimeSlotsAsync(Guid departmentId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var request = new
                {
                    DepartmentId = departmentId,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Appointment/GetAppointmentExsit", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var timeSlots = JsonSerializer.Deserialize<List<TimeSlotOption>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ServiceResult<List<TimeSlotOption>>
                    {
                        Success = true,
                        Data = timeSlots ?? new List<TimeSlotOption>()
                    };
                }

                return new ServiceResult<List<TimeSlotOption>>
                {
                    Success = false,
                    ErrorMessage = "Không thể tải danh sách khung giờ",
                    Data = new List<TimeSlotOption>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting time slots");
                return new ServiceResult<List<TimeSlotOption>>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Data = new List<TimeSlotOption>()
                };
            }
        }

        public async Task<ServiceResult<List<TimeSlotOption>>> CheckTimeSlotAvailabilityAsync(Guid departmentId, DateTime date)
        {
            try
            {
                var request = new
                {
                    DepartmentId = departmentId,
                    Date = date
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Appointment/GetAppointmentExsit", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var timeSlots = JsonSerializer.Deserialize<List<TimeSlotOption>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ServiceResult<List<TimeSlotOption>>
                    {
                        Success = true,
                        Data = timeSlots ?? new List<TimeSlotOption>()
                    };
                }

                return new ServiceResult<List<TimeSlotOption>>
                {
                    Success = false,
                    ErrorMessage = "Không thể kiểm tra tình trạng khung giờ",
                    Data = new List<TimeSlotOption>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking time slot availability");
                return new ServiceResult<List<TimeSlotOption>>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Data = new List<TimeSlotOption>()
                };
            }
        }

        public async Task<ServiceResult<List<TimeSlotOption>>> GetTimeSlotsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/TimeSlot");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var timeSlots = JsonSerializer.Deserialize<List<TimeSlotOption>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ServiceResult<List<TimeSlotOption>>
                    {
                        Success = true,
                        Data = timeSlots ?? new List<TimeSlotOption>()
                    };
                }

                _logger.LogWarning("Failed to get time slots from API. StatusCode: {StatusCode}", response.StatusCode);
                return new ServiceResult<List<TimeSlotOption>>
                {
                    Success = false,
                    ErrorMessage = "Không thể tải danh sách khung giờ từ máy chủ",
                    Data = new List<TimeSlotOption>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting time slots");
                return new ServiceResult<List<TimeSlotOption>>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Data = new List<TimeSlotOption>()
                };
            }
        }
    }
}
