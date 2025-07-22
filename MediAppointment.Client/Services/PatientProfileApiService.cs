using MediAppointment.Client.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using MediAppointment.Client.Models.MedicalRecord;
using MediAppointment.Client.Models.Patient;

namespace MediAppointment.Client.Services
{
    public interface IPatientProfileApiService
    {
        Task<PatientViewModel?> GetPatientProfileAsync();
        Task<bool> AddMedicalRecordAsync(CreateMedicalRecordRequest dto);
    }

    public class PatientProfileApiService : BaseApiService, IPatientProfileApiService
    {
        private readonly string _baseUrl;

        public PatientProfileApiService(HttpClient httpClient, 
            IHttpContextAccessor httpContextAccessor, 
            IConfiguration configuration)
            : base(httpClient, httpContextAccessor)
        {
            _baseUrl = configuration["ApiBaseUrl"]!;
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<PatientViewModel?> GetPatientProfileAsync()
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync("api/Patient/profile");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PatientViewModel>();
            }

            return null;
        }


        public async Task<bool> AddMedicalRecordAsync(CreateMedicalRecordRequest dto)
        {
            SetAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/Patient/add-medical-record", dto);
            return response.IsSuccessStatusCode;
        }
    }
 }
