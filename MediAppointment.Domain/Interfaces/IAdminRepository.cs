using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Entities.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace MediAppointment.Domain.Interfaces
{
    public interface IAdminRepository
    {
        Task<IEnumerable<Admin>> GetAllAsync();
        Task<Admin?> GetByIdAsync(Guid id);
        Task AddAsync(Admin admin);
        Task UpdateAsync(Admin admin);
        Task DeleteAsync(Guid id);
        // Quản lý người dùng
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task SetUserStatusAsync(Guid userId, bool isActive);
        Task ChangeUserRoleAsync(Guid userId, string newRole);
    }
}
