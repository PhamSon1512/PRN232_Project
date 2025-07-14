using System.Net.Http.Headers;

namespace MediAppointment.Client.Services
{
    public abstract class BaseApiService
    {
        protected readonly HttpClient _httpClient;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        protected void SetAuthHeader()
        {
            // Lấy token từ session và set vào Authorization header
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
