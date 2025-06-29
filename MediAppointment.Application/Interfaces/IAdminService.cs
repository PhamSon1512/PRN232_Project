using MediAppointment.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediAppointment.Application.DTOs.AdminDTOs;
using MediAppointment.Application.DTOs.UserDTOs;
using System;

namespace MediAppointment.Application.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<AdminDTO>> GetAllAdminsAsync();
        Task<AdminDTO?> GetAdminByIdAsync(Guid id);
        Task CreateAdminAsync(AdminDTO adminDto);
        Task UpdateAdminAsync(AdminDTO adminDto);
        Task DeleteAdminAsync(Guid id);
        // Quản lý người dùng
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO?> GetUserByIdAsync(Guid id);
        Task SetUserStatusAsync(Guid userId, bool isActive);
        Task ChangeUserRoleAsync(Guid userId, string newRole);
    }
}
