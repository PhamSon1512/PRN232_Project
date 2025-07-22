using System.IdentityModel.Tokens.Jwt;
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
        Task<ApiResponse<DoctorStatusModel>> GetDoctorByIdAsync(Guid doctorId);
        Task<ApiResponse<DoctorViewModel>> UpdateDoctorProfileAsync(Guid doctorId, DoctorUpdateModel dto);
        Task<ApiResponse<bool>> CreateDoctorAsync(DoctorCreateModel dto);
        Task<ApiResponse<bool>> DeleteDoctorAsync(Guid doctorId);
        Task<ApiResponse<ManagerViewModel>> GetManagerProfileAsync();
        Task<ApiResponse<bool>> UpdateManagerProfileAsync(ManagerUpdateProfile dto);
        Task<Dictionary<DateTime, List<ManagerScheduleSlot>>> GetWeeklyScheduleAsync(Guid? departmentId, Guid? roomId, Guid? doctorId, int year, int week);
    }

    public class ManagerService : IManagerService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ManagerService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<ApiResponse<DoctorStatusModel>> GetDoctorByIdAsync(Guid doctorId)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"/api/Manager/doctors/{doctorId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var doctor = JsonSerializer.Deserialize<DoctorStatusModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new ApiResponse<DoctorStatusModel> { Success = true, Data = doctor };
                }

                return new ApiResponse<DoctorStatusModel> { Success = false, ErrorMessage = "Không thể tải thông tin bác sĩ" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetDoctorByIdAsync: {ex.Message}");
                return new ApiResponse<DoctorStatusModel> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<DoctorViewModel>> UpdateDoctorProfileAsync(Guid doctorId, DoctorUpdateModel dto)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/api/Manager/update/{doctorId}", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: Status {response.StatusCode}, Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var updatedDoctor = JsonSerializer.Deserialize<DoctorViewModel>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    Console.WriteLine($"Doctor updated successfully with ID: {doctorId}");
                    return new ApiResponse<DoctorViewModel> { Success = true, Data = updatedDoctor };
                }

                Dictionary<string, string>? errorObj = null;
                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    try
                    {
                        errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Failed to deserialize error content as JSON: {ex.Message}, Content: {responseContent}");
                        return new ApiResponse<DoctorViewModel>
                        {
                            Success = false,
                            ErrorMessage = $"Cập nhật hồ sơ bác sĩ thất bại: {responseContent}"
                        };
                    }
                }

                return new ApiResponse<DoctorViewModel>
                {
                    Success = false,
                    ErrorMessage = errorObj?.GetValueOrDefault("message") ?? $"Cập nhật hồ sơ bác sĩ thất bại: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateDoctorProfileAsync: {ex.Message}");
                return new ApiResponse<DoctorViewModel> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> CreateDoctorAsync(DoctorCreateModel dto)
        {
            try
            {
                SetAuthHeader();
                var payload = JsonSerializer.Serialize(dto);
                Console.WriteLine($"Sending payload: {payload}");
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Manager/create", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: Status {response.StatusCode}, Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Doctor created successfully");
                    return new ApiResponse<bool> { Success = true, Data = true };
                }

                Dictionary<string, string>? errorObj = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Failed to deserialize error content as JSON: {ex.Message}, Content: {responseContent}");
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        ErrorMessage = $"Tạo bác sĩ thất bại: {responseContent}"
                    };
                }

                return new ApiResponse<bool>
                {
                    Success = false,
                    ErrorMessage = errorObj?.GetValueOrDefault("message") ?? $"Tạo bác sĩ thất bại: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateDoctorAsync: {ex.Message}");
                return new ApiResponse<bool> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> DeleteDoctorAsync(Guid doctorId)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.DeleteAsync($"/api/Manager/{doctorId}");

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: Status {response.StatusCode}, Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Doctor deleted successfully with ID: {doctorId}");
                    return new ApiResponse<bool> { Success = true, Data = true };
                }

                Dictionary<string, string>? errorObj = null;
                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    try
                    {
                        errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Failed to deserialize error content as JSON: {ex.Message}, Content: {responseContent}");
                        return new ApiResponse<bool>
                        {
                            Success = false,
                            ErrorMessage = $"Xóa bác sĩ thất bại: {responseContent}"
                        };
                    }
                }

                return new ApiResponse<bool>
                {
                    Success = false,
                    ErrorMessage = errorObj?.GetValueOrDefault("message") ?? $"Xóa bác sĩ thất bại: {responseContent}"
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

                if (dto.ManagerId == null || dto.ManagerId == Guid.Empty)
                {
                    var managerId = GetManagerIdFromSession();
                    if (managerId == null)
                    {
                        Console.WriteLine("Failed to retrieve ManagerId from session.");
                        return new ApiResponse<bool>
                        {
                            Success = false,
                            ErrorMessage = "Không thể xác định ID manager từ session."
                        };
                    }
                    dto.ManagerId = managerId.Value;
                    Console.WriteLine($"ManagerId set to: {dto.ManagerId}");
                }

                if (string.IsNullOrWhiteSpace(dto.FullName))
                {
                    Console.WriteLine("FullName is empty or whitespace.");
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        ErrorMessage = "FullName cannot be empty."
                    };
                }

                if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                {
                    Console.WriteLine("PhoneNumber is empty or whitespace.");
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        ErrorMessage = "PhoneNumber cannot be empty."
                    };
                }

                var serverDto = new
                {
                    ManagerId = dto.ManagerId,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber
                };

                Console.WriteLine($"Sending API request with payload: {JsonSerializer.Serialize(serverDto)}");
                var content = new StringContent(JsonSerializer.Serialize(serverDto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("/api/Manager/UpdateManagerProfile", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("UpdateManagerProfileAsync: API call successful");
                    return new ApiResponse<bool> { Success = true };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error: Status {response.StatusCode}, Content: {errorContent}");

                // Kiểm tra nếu errorContent là JSON
                Dictionary<string, string>? errorObj = null;
                try
                {
                    errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException)
                {
                    Console.WriteLine($"Failed to deserialize errorContent as JSON: {errorContent}");
                    // Trả về errorContent như text thuần
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        ErrorMessage = $"Cập nhật hồ sơ thất bại: {errorContent}"
                    };
                }

                return new ApiResponse<bool>
                {
                    Success = false,
                    ErrorMessage = errorObj?.GetValueOrDefault("message") ?? $"Cập nhật hồ sơ thất bại: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateManagerProfileAsync: {ex.Message}");
                return new ApiResponse<bool>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private Guid? GetManagerIdFromSession()
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

                Console.WriteLine("JWT Claims in ManagerService:");
                foreach (var claim in jwt.Claims)
                {
                    Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
                }

                var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    Console.WriteLine("Claim 'nameid' not found in token");
                    return null;
                }

                if (!Guid.TryParse(userIdClaim, out var managerId))
                {
                    Console.WriteLine($"Failed to parse 'nameid' to Guid: {userIdClaim}");
                    return null;
                }

                Console.WriteLine($"ManagerId from token: {managerId}");
                return managerId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting ManagerId from session: {ex.Message}");
                return null;
            }
        }

        public async Task<Dictionary<DateTime, List<ManagerScheduleSlot>>> GetWeeklyScheduleAsync(Guid? departmentId, Guid? roomId, Guid? doctorId, int year, int week)
        {
            try
            {
                SetAuthHeader();
                var query = $"?year={year}&week={week}";
                if (departmentId.HasValue && departmentId != Guid.Empty) query += $"&departmentId={departmentId}";
                if (roomId.HasValue && roomId != Guid.Empty) query += $"&roomId={roomId}";
                if (doctorId.HasValue && doctorId != Guid.Empty) query += $"&doctorId={doctorId}";

                var response = await _httpClient.GetAsync($"/api/Manager/WeeklySchedule{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var schedule = JsonSerializer.Deserialize<Dictionary<DateTime, List<ManagerScheduleSlot>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return schedule ?? new Dictionary<DateTime, List<ManagerScheduleSlot>>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Không thể lấy lịch làm việc: {errorContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong GetWeeklyScheduleAsync: {ex.Message}");
                throw new Exception($"Lỗi khi lấy lịch làm việc: {ex.Message}", ex);
            }
        }
    }
}