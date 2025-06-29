using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MediAppointment.Client.Models;
using Microsoft.Extensions.Configuration;

namespace MediAppointment.Client.Services
{
    public class AdminService : IAdminService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        public AdminService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiBaseUrl"] + "/api/admin";
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<UserViewModel>>($"{_baseUrl}/users");
        }

        public async Task SetUserStatusAsync(Guid id, bool isActive)
        {
            await _httpClient.PutAsync($"{_baseUrl}/users/{id}/status?isActive={isActive}", null);
        }

        public async Task ChangeUserRoleAsync(Guid id, string newRole)
        {
            await _httpClient.PutAsync($"{_baseUrl}/users/{id}/role?newRole={newRole}", null);
        }
    }
}
