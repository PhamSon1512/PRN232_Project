using MediAppointment.Client.Models.DoctorSchedule;
using System.Text;
using System.Text.Json;

namespace MediAppointment.Client.Services
{
    public interface IScheduleService
    {
        Task<ServiceResult<string>> CreateScheduleAsync(object scheduleData);
        Task<ServiceResult<string>> CreateDoctorScheduleAsync(List<ScheduleCreateRequest> requests);
        Task<ServiceResult<string>> DeleteScheduleAsync(ScheduleDeleteRequest request);
        Task<ServiceResult<string>> DeleteDoctorScheduleAsync(ScheduleDeleteRequest request);
        Task<ServiceResult<object>> GetScheduleAsync(object request);
        Task<ServiceResult<List<ScheduleSlot>>> GetDoctorScheduleAsync(Guid roomId, int year, int week);
    }

    public class ScheduleService : IScheduleService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ScheduleService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ScheduleService(IHttpClientFactory httpClientFactory, ILogger<ScheduleService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResult<string>> CreateScheduleAsync(object scheduleData)
        {
            try
            {
                var json = JsonSerializer.Serialize(scheduleData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/DoctorSchedule", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return new ServiceResult<string>
                    {
                        Success = true,
                        Data = "Đăng ký lịch làm việc thành công"
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API Error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                
                return new ServiceResult<string>
                {
                    Success = false,
                    ErrorMessage = "Không thể đăng ký lịch làm việc"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule");
                return new ServiceResult<string>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ServiceResult<string>> CreateDoctorScheduleAsync(List<ScheduleCreateRequest> requests)
        {
            try
            {
                // Get JWT token from session
                var token = GetJwtTokenFromSession();
                if (string.IsNullOrEmpty(token))
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        ErrorMessage = "Vui lòng đăng nhập lại"
                    };
                }

                var json = JsonSerializer.Serialize(requests);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add Authorization header
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync("api/DoctorSchedule/CreateSchedule", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<dynamic>(responseContent);
                    
                    return new ServiceResult<string>
                    {
                        Success = true,
                        Data = "Đăng ký lịch làm việc thành công"
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API Error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                
                return new ServiceResult<string>
                {
                    Success = false,
                    ErrorMessage = "Không thể đăng ký lịch làm việc"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating doctor schedule");
                return new ServiceResult<string>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private string? GetJwtTokenFromSession()
        {
            // Get JWT token from session
            return _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
        }

        public async Task<ServiceResult<string>> DeleteScheduleAsync(ScheduleDeleteRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "api/DoctorSchedule")
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(deleteRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return new ServiceResult<string>
                    {
                        Success = true,
                        Data = "Xóa lịch làm việc thành công"
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API Error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                
                return new ServiceResult<string>
                {
                    Success = false,
                    ErrorMessage = "Không thể xóa lịch làm việc"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule");
                return new ServiceResult<string>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ServiceResult<string>> DeleteDoctorScheduleAsync(ScheduleDeleteRequest request)
        {
            try
            {
                // Get JWT token from session
                var token = GetJwtTokenFromSession();
                if (string.IsNullOrEmpty(token))
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        ErrorMessage = "Vui lòng đăng nhập lại"
                    };
                }

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/DoctorSchedule/DeleteSchedule")
                {
                    Content = content
                };

                // Add Authorization header
                httpRequest.Headers.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return new ServiceResult<string>
                    {
                        Success = true,
                        Data = "Xóa lịch làm việc thành công"
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API Error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                
                return new ServiceResult<string>
                {
                    Success = false,
                    ErrorMessage = "Không thể xóa lịch làm việc"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting doctor schedule");
                return new ServiceResult<string>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ServiceResult<object>> GetScheduleAsync(object request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/DoctorSchedule/GetDoctorSchedule", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var scheduleData = JsonSerializer.Deserialize<object>(responseContent);
                    
                    return new ServiceResult<object>
                    {
                        Success = true,
                        Data = scheduleData
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API Error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                
                return new ServiceResult<object>
                {
                    Success = false,
                    ErrorMessage = "Không thể tải lịch làm việc"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule");
                return new ServiceResult<object>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ServiceResult<List<ScheduleSlot>>> GetDoctorScheduleAsync(Guid roomId, int year, int week)
        {
            try
            {
                // Get JWT token from session
                var token = GetJwtTokenFromSession();
                if (string.IsNullOrEmpty(token))
                {
                    return new ServiceResult<List<ScheduleSlot>>
                    {
                        Success = false,
                        ErrorMessage = "Vui lòng đăng nhập lại",
                        Data = new List<ScheduleSlot>()
                    };
                }

                // Add Authorization header
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"api/DoctorSchedule/GetSchedule?roomId={roomId}&year={year}&week={week}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var scheduleData = JsonSerializer.Deserialize<List<ScheduleSlot>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ServiceResult<List<ScheduleSlot>>
                    {
                        Success = true,
                        Data = scheduleData ?? new List<ScheduleSlot>()
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API Error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                
                return new ServiceResult<List<ScheduleSlot>>
                {
                    Success = false,
                    ErrorMessage = "Không thể tải lịch làm việc",
                    Data = new List<ScheduleSlot>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor schedule");
                return new ServiceResult<List<ScheduleSlot>>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Data = new List<ScheduleSlot>()
                };
            }
        }
    }

    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
