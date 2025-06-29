using MediAppointment.Application.DTOs.AdminDTOs;
using MediAppointment.Application.DTOs.UserDTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediAppointment.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<IEnumerable<AdminDTO>> GetAllAdminsAsync()
        {
            var admins = await _adminRepository.GetAllAsync();
            return admins.Select(a => new AdminDTO
            {
                Id = a.Id,
                FullName = a.FullName,
                Gender = a.Gender,
                DateOfBirth = a.DateOfBirth,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                Status = a.Status.ToString()
            });
        }

        public async Task<AdminDTO?> GetAdminByIdAsync(Guid id)
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            if (admin == null) return null;
            return new AdminDTO
            {
                Id = admin.Id,
                FullName = admin.FullName,
                Gender = admin.Gender,
                DateOfBirth = admin.DateOfBirth,
                Email = admin.Email,
                PhoneNumber = admin.PhoneNumber,
                Status = admin.Status.ToString()
            };
        }

        public async Task CreateAdminAsync(AdminDTO adminDto)
        {
            var admin = new Admin
            {
                Id = Guid.NewGuid(),
                FullName = adminDto.FullName,
                Gender = adminDto.Gender,
                DateOfBirth = adminDto.DateOfBirth,
                Email = adminDto.Email,
                PhoneNumber = adminDto.PhoneNumber,
                Status = Enum.Parse<Domain.Enums.Status>(adminDto.Status)
            };
            await _adminRepository.AddAsync(admin);
        }

        public async Task UpdateAdminAsync(AdminDTO adminDto)
        {
            var admin = await _adminRepository.GetByIdAsync(adminDto.Id);
            if (admin == null) return;
            admin.FullName = adminDto.FullName;
            admin.Gender = adminDto.Gender;
            admin.DateOfBirth = adminDto.DateOfBirth;
            admin.Email = adminDto.Email;
            admin.PhoneNumber = adminDto.PhoneNumber;
            admin.Status = Enum.Parse<Domain.Enums.Status>(adminDto.Status);
            await _adminRepository.UpdateAsync(admin);
        }

        public async Task DeleteAdminAsync(Guid id)
        {
            await _adminRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _adminRepository.GetAllUsersAsync();
            return users.Select(u => new UserDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                Gender = u.Gender,
                DateOfBirth = u.DateOfBirth,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Status = u.Status.ToString(),
                Role = u.GetType().Name
            });
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid id)
        {
            var user = await _adminRepository.GetUserByIdAsync(id);
            if (user == null) return null;
            return new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Status = user.Status.ToString(),
                Role = user.GetType().Name
            };
        }

        public async Task SetUserStatusAsync(Guid userId, bool isActive)
        {
            await _adminRepository.SetUserStatusAsync(userId, isActive);
        }

        public async Task ChangeUserRoleAsync(Guid userId, string newRole)
        {
            await _adminRepository.ChangeUserRoleAsync(userId, newRole);
        }
    }
}
