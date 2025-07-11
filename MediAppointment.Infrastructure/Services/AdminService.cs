using MediAppointment.Application.DTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediAppointment.Domain.Enums;
using MediAppointment.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using MediAppointment.Application.DTOs.Pages;
using Microsoft.EntityFrameworkCore;
using MediAppointment.Application.DTOs.ManagerDTOs;
using Microsoft.AspNetCore.Mvc;
using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Infrastructure.Data;

namespace MediAppointment.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<UserIdentity> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;

        public AdminService(UserManager<UserIdentity> userManager, ApplicationDbContext dbContext, IEmailService emailService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _emailService = emailService;
        }

        public async Task<PagedResult<DoctorManagerDto>> GetAllDoctorsAndManagersAsync(string text = "", int page = 1, int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            // Get all users with Doctor or Manager roles
            var doctorUsers = await _userManager.GetUsersInRoleAsync("Doctor");
            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
            var allUsers = doctorUsers.Concat(managerUsers).Distinct();

            // Filtering
            if (!string.IsNullOrWhiteSpace(text))
            {
                text = text.Trim().ToLower();
                allUsers = allUsers.Where(u => u.FullName.ToLower().Contains(text) || u.Email.ToLower().Contains(text));
            }

            var totalCount = allUsers.Count();

            // Apply pagination
            var users = allUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var adminDtos = new List<DoctorManagerDto>();
            foreach (var user in users)
            {
                var userEntity = await _dbContext.Set<User>().FirstOrDefaultAsync(u => u.UserIdentityId == user.Id);

                adminDtos.Add(new DoctorManagerDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = doctorUsers.Any(d => d.Id == user.Id) ? "Doctor" : "Manager",
                    DateOfBirth = userEntity?.DateOfBirth ?? DateTime.MinValue,
                    Gender = userEntity?.Gender ?? false,
                    IsActive = userEntity?.Status == Status.Active
                });
            }

            return new PagedResult<DoctorManagerDto>
            {
                Items = adminDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // admin profile
        public async Task<object> GetAdminProfileAsync(Guid adminId)
        {
            var admin = await _userManager.FindByIdAsync(adminId.ToString());
            if (admin == null) throw new Exception("Admin not found");

            var roles = await _userManager.GetRolesAsync(admin);
            var role = roles.FirstOrDefault() ?? "Unknown";

            return new
            {
                Id = admin.Id,
                Email = admin.Email,
                FullName = admin.FullName,
                PhoneNumber = admin.PhoneNumber,
                UserName = admin.UserName,
                Role = role
            };
        }

        // create manager
        public async Task<object> CreateDoctorToManagerAsync(ManagerCreateDto dto)
        {
            var doctor = await _userManager.FindByIdAsync(dto.DoctorId.ToString());
            if (doctor == null)
                throw new Exception($"User with Id {dto.DoctorId} does not exist.");
            if (!(await _userManager.IsInRoleAsync(doctor, "Doctor")))
                throw new Exception($"User with Id {dto.DoctorId} does not have the Doctor role.");

            // Update only editable fields
            if (!string.IsNullOrEmpty(dto.FullName) && dto.FullName != doctor.FullName)
                doctor.FullName = dto.FullName;
            if (!string.IsNullOrEmpty(dto.PhoneNumber) && dto.PhoneNumber != doctor.PhoneNumber)
                doctor.PhoneNumber = dto.PhoneNumber;

            var result = await _userManager.UpdateAsync(doctor);
            if (!result.Succeeded) throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            // Update role from Doctor to Manager (validate NewRole if provided)
            if (!string.IsNullOrEmpty(dto.NewRole) && dto.NewRole != "Manager")
                throw new Exception("New role must be Manager");
            await _userManager.AddToRoleAsync(doctor, "Manager");
            await _userManager.RemoveFromRoleAsync(doctor, "Doctor");

            // Delete the Doctor record from the User table
            var doctorEntity = await _dbContext.Set<User>()
                .OfType<Doctor>()
                .FirstOrDefaultAsync(d => d.UserIdentityId == dto.DoctorId);
            if (doctorEntity != null)
            {
                _dbContext.Set<User>().Remove(doctorEntity);
                await _dbContext.SaveChangesAsync();
            }
            await _emailService.SendAsync(doctor.Email, "Role Updated", "Your role has been updated to Manager by an Admin. Welcome!");
            return new
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                Email = doctor.Email,
                PhoneNumber = doctor.PhoneNumber,
                UserName = doctor.UserName,
                Role = "Manager"
            };
        }

        public async Task<object> UpdateManagerProfileAsync(ManagerUpdateDto dto)
        {
            var manager = await _userManager.FindByIdAsync(dto.DoctorId.ToString());
            if (manager == null)
                throw new Exception($"User with Id {dto.DoctorId} does not exist.");
            if (!(await _userManager.IsInRoleAsync(manager, "Manager")))
                throw new Exception($"User with Id {dto.DoctorId} does not have the Manager role.");

            // Update only editable fields if provided and different
            if (!string.IsNullOrEmpty(dto.FullName) && dto.FullName != manager.FullName)
                manager.FullName = dto.FullName;
            if (!string.IsNullOrEmpty(dto.PhoneNumber) && dto.PhoneNumber != manager.PhoneNumber)
                manager.PhoneNumber = dto.PhoneNumber;

            var result = await _userManager.UpdateAsync(manager);
            if (!result.Succeeded) throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
            await _emailService.SendAsync(manager.Email, "Profile Updated", $"Your profile has been updated.");

            return new
            {
                Id = manager.Id,
                FullName = manager.FullName,
                Email = manager.Email,
                PhoneNumber = manager.PhoneNumber,
                UserName = manager.UserName,
                Role = await _userManager.GetRolesAsync(manager) // Returns the current role(s)
            };
        }

        public async Task<DoctorManagerDto> GetUserByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                throw new Exception($"User with Id {id} does not exist.");

            var userEntity = await _dbContext.Set<User>().FirstOrDefaultAsync(u => u.UserIdentityId == user.Id);
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.Contains("Doctor") ? "Doctor" : roles.Contains("Manager") ? "Manager" : "Unknown";

            return new DoctorManagerDto
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = role,
                DateOfBirth = userEntity?.DateOfBirth ?? DateTime.MinValue,
                Gender = userEntity?.Gender ?? false,
                IsActive = userEntity?.Status == Status.Active
            };
        }

        public async Task<object> UpdateAdminProfileAsync(AdminUpdateProfileDto dto)
        {
            var admin = await _userManager.FindByIdAsync(dto.AdminId.ToString());
            if (admin == null)
                throw new Exception($"Admin with Id {dto.AdminId} does not exist.");
            if (!(await _userManager.IsInRoleAsync(admin, "Admin")))
                throw new Exception($"User with Id {dto.AdminId} does not have the Admin role.");

            if (!string.IsNullOrEmpty(dto.FullName) && dto.FullName != admin.FullName)
                admin.FullName = dto.FullName;
            if (!string.IsNullOrEmpty(dto.PhoneNumber) && dto.PhoneNumber != admin.PhoneNumber)
                admin.PhoneNumber = dto.PhoneNumber;

            var result = await _userManager.UpdateAsync(admin);
            if (!result.Succeeded) throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
            await _emailService.SendAsync(admin.Email, "Profile Updated", $"Your profile has been updated.");

            return new
            {
                Id = admin.Id,
                FullName = admin.FullName,
                Email = admin.Email,
                PhoneNumber = admin.PhoneNumber,
                UserName = admin.UserName,
                Role = await _userManager.GetRolesAsync(admin)
            };
        }
    }
}
