using MediAppointment.Client.Models.Appointment;
using AdminModels = MediAppointment.Client.Models.Admin;
using System.Text.Json;

namespace MediAppointment.Client.Services
{
    public interface IDepartmentService
    {
        Task<ServiceResult<List<DepartmentOption>>> GetDepartmentsAsync();
        Task<ServiceResult<List<AdminModels.RoomOption>>> GetRoomsByDepartmentAsync(Guid departmentId);
        Task<ServiceResult<List<AdminModels.DoctorOption>>> GetDoctorsByDepartmentAsync(Guid departmentId);
    }

    public class DepartmentService : IDepartmentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(IHttpClientFactory httpClientFactory, ILogger<DepartmentService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
        }

        public async Task<ServiceResult<List<DepartmentOption>>> GetDepartmentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Department");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var departments = JsonSerializer.Deserialize<List<DepartmentOption>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ServiceResult<List<DepartmentOption>>
                    {
                        Success = true,
                        Data = departments ?? new List<DepartmentOption>()
                    };
                }

                return new ServiceResult<List<DepartmentOption>>
                {
                    Success = false,
                    ErrorMessage = "Không thể tải danh sách khoa",
                    Data = new List<DepartmentOption>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments");
                return new ServiceResult<List<DepartmentOption>>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Data = new List<DepartmentOption>()
                };
            }
        }

        public async Task<ServiceResult<List<AdminModels.RoomOption>>> GetRoomsByDepartmentAsync(Guid departmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Room/department/{departmentId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var rooms = JsonSerializer.Deserialize<List<AdminModels.RoomOption>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ServiceResult<List<AdminModels.RoomOption>>
                    {
                        Success = true,
                        Data = rooms ?? new List<AdminModels.RoomOption>()
                    };
                }

                return new ServiceResult<List<AdminModels.RoomOption>>
                {
                    Success = false,
                    ErrorMessage = "Không thể tải danh sách phòng",
                    Data = new List<AdminModels.RoomOption>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rooms");
                return new ServiceResult<List<AdminModels.RoomOption>>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Data = new List<AdminModels.RoomOption>()
                };
            }
        }

        public async Task<ServiceResult<List<AdminModels.DoctorOption>>> GetDoctorsByDepartmentAsync(Guid departmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Doctor/department/{departmentId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var doctors = JsonSerializer.Deserialize<List<AdminModels.DoctorOption>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ServiceResult<List<AdminModels.DoctorOption>>
                    {
                        Success = true,
                        Data = doctors ?? new List<AdminModels.DoctorOption>()
                    };
                }

                return new ServiceResult<List<AdminModels.DoctorOption>>
                {
                    Success = false,
                    ErrorMessage = "Không thể tải danh sách bác sĩ",
                    Data = new List<AdminModels.DoctorOption>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctors");
                return new ServiceResult<List<AdminModels.DoctorOption>>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Data = new List<AdminModels.DoctorOption>()
                };
            }
        }
    }
}
