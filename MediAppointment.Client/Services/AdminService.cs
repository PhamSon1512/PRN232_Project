using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MediAppointment.Client.Models;
using Microsoft.Extensions.Configuration;

namespace MediAppointment.Client.Services
{public enum Status
    {
        Inactive = 0,
        Active = 1,
        Pending = 2,
        Deleted = 3
    }

    public interface IAdminService
    {
        Task<IEnumerable<UserViewModel>> GetAllUsersAsync();
        Task UpdateUserStatusAsync(Guid id, Status status);
        Task UpdateUserRoleAsync(Guid id, string newRole);
    }
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
            return await _httpClient.GetFromJsonAsync<IEnumerable<UserViewModel>>($"{_baseUrl}/users") ?? new List<UserViewModel>();
        }

        public async Task UpdateUserStatusAsync(Guid id, Status status)
        {
            await _httpClient.PutAsync($"{_baseUrl}/users/{id}/status?status={status}", null);
        }

        public async Task UpdateUserRoleAsync(Guid id, string newRole)
        {
            await _httpClient.PutAsync($"{_baseUrl}/users/{id}/role?newRole={newRole}", null);
        }
    }
}
