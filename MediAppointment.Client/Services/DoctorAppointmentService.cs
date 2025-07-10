using MediAppointment.Client.Models;
using MediAppointment.Client.Models.Doctor;
using MediAppointment.Client.Models.MedicalRecord;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace MediAppointment.Client.Services
{
    public interface IDoctorAppointmentService
    {
        Task<IEnumerable<Doctor_AssignedSlotViewModel>> GetAssignedSlotsAsync();
        Task<IEnumerable<Doctor_AssignedSlotViewModel>> GetAssignedSlotsAsync(int year, int week);
        Task<Doctor_SlotAppointmentViewModel?> GetAppointmentsBySlotAsync(Guid roomTimeSlotId);
        Task<Doctor_PatientDetailViewModel?> GetPatientDetailAsync(Guid patientId);
        Task<bool> CreateMedicalRecordAsync(CreateMedicalRecordRequest request);

    }

    public class DoctorAppointmentService: IDoctorAppointmentService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl;

        public DoctorAppointmentService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _baseUrl = configuration["ApiBaseUrl"] + "/api/DoctorAppoinment";
        }

        private void SetAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"];

            if (!string.IsNullOrEmpty(token))
            {
                // Clear header trước để tránh trùng lặp hoặc xung đột
                _httpClient.DefaultRequestHeaders.Authorization = null;

                // Gán lại header mới mỗi lần gọi
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }




        public async Task<IEnumerable<Doctor_AssignedSlotViewModel>> GetAssignedSlotsAsync()
        {
            try
            {
                SetAuthHeader();

                var response = await _httpClient.GetAsync($"{_baseUrl}/assigned-slots");
                response.EnsureSuccessStatusCode();

                var slots = await response.Content.ReadFromJsonAsync<IEnumerable<Doctor_AssignedSlotViewModel>>();
                return slots ?? new List<Doctor_AssignedSlotViewModel>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Lỗi HTTP: {ex.Message}");
                return new List<Doctor_AssignedSlotViewModel>();
            }
        }



        public async Task<IEnumerable<Doctor_AssignedSlotViewModel>> GetAssignedSlotsAsync(int year, int week)
        {
            SetAuthHeader();
            var url = $"{_baseUrl}/assigned-slots?year={year}&week={week}";
            return await _httpClient.GetFromJsonAsync<IEnumerable<Doctor_AssignedSlotViewModel>>(url)
                   ?? new List<Doctor_AssignedSlotViewModel>();
        }

        public async Task<Doctor_SlotAppointmentViewModel?> GetAppointmentsBySlotAsync(Guid roomTimeSlotId)
        {
            SetAuthHeader();
            var url = $"{_baseUrl}/appointments-by-slot/{roomTimeSlotId}";
            return await _httpClient.GetFromJsonAsync<Doctor_SlotAppointmentViewModel>(url);
        }

        public async Task<Doctor_PatientDetailViewModel?> GetPatientDetailAsync(Guid patientId)
        {
            SetAuthHeader();
            var url = $"{_baseUrl}/patient-detail/{patientId}";
            return await _httpClient.GetFromJsonAsync<Doctor_PatientDetailViewModel>(url);
        }

        public async Task<bool> CreateMedicalRecordAsync(CreateMedicalRecordRequest request)
        {
            SetAuthHeader();
            var url = $"{_baseUrl}/create";
            var response = await _httpClient.PostAsJsonAsync(url, request);

            // 👉 Thêm dòng log này để xem chi tiết phản hồi lỗi
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"CreateMedicalRecordAsync - StatusCode: {response.StatusCode}, Content: {content}");

            return response.IsSuccessStatusCode;
        }


    }
}
