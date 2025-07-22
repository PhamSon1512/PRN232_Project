using MediAppointment.Client.Models.Appointment;
using System.Net.Http.Headers;

namespace MediAppointment.Client.Services
{

    public interface IBookingDoctorService
    {
        Task<bool> BookAppointmentAsync(BookingDoctorCreate request);
        Task<bool> CancelBookingAsync(Guid appointmentId);
        Task<IEnumerable<BookingDoctorView>> GetBookingsByPatientAsync();

    }

    public class BookingDoctorService : IBookingDoctorService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl;

        public BookingDoctorService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _baseUrl = configuration["ApiBaseUrl"] + "/api/Appointment";
        }

        private void SetAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // 1. Đặt lịch hẹn với bác sĩ
        public async Task<bool> BookAppointmentAsync(BookingDoctorCreate request)
        {
            SetAuthHeader();
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/BookAppointmentWithDoctor", request);
            return response.IsSuccessStatusCode;
        }

        // 2. Hủy lịch hẹn
        public async Task<bool> CancelBookingAsync(Guid appointmentId)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/CancelBookingDoctor/{appointmentId}");
            return response.IsSuccessStatusCode;
        }

       

        // 5. Lấy danh sách các lịch hẹn bác sĩ của bệnh nhân
        public async Task<IEnumerable<BookingDoctorView>> GetBookingsByPatientAsync()
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"{_baseUrl}/MyDoctorBookings");

            if (!response.IsSuccessStatusCode)
                return new List<BookingDoctorView>();

            return await response.Content.ReadFromJsonAsync<IEnumerable<BookingDoctorView>>()
                   ?? new List<BookingDoctorView>();
        }

    }
}
