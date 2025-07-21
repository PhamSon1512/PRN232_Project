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

            var doctorUsers = await _userManager.GetUsersInRoleAsync("Doctor");
            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
            var allUsers = doctorUsers.Concat(managerUsers).Distinct();

            if (!string.IsNullOrWhiteSpace(text))
            {
                text = text.Trim().ToLower();
                allUsers = allUsers.Where(u => u.FullName.ToLower().Contains(text) || u.Email.ToLower().Contains(text));
            }

            var totalCount = allUsers.Count();

            var users = allUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var adminDtos = new List<DoctorManagerDto>();
            foreach (var user in users)
            {
                adminDtos.Add(new DoctorManagerDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = doctorUsers.Any(d => d.Id == user.Id) ? "Doctor" : "Manager",
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    IsActive = user.Status == Status.Active
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
                throw new Exception($"Người dùng với Id {dto.DoctorId} không tồn tại.");
            if (!(await _userManager.IsInRoleAsync(doctor, "Doctor")))
                throw new Exception($"Người dùng với Id {dto.DoctorId} không có vai trò Doctor.");
            if (dto.IsActive.HasValue)
                doctor.Status = dto.IsActive.Value ? Status.Active : Status.Inactive;

            var doctorEntity = await _dbContext.Set<User>()
                .OfType<Doctor>()
                .FirstOrDefaultAsync(d => d.UserIdentityId == dto.DoctorId);
            if (doctorEntity != null)
            {
                doctor.Gender = doctorEntity.Gender;
                doctor.DateOfBirth = doctorEntity.DateOfBirth;
                await _userManager.UpdateAsync(doctor);
            }
            else
            {
                doctor.Gender = true;
                doctor.DateOfBirth = DateTime.MinValue;
                await _userManager.UpdateAsync(doctor);
            }

            if (!string.IsNullOrEmpty(dto.FullName) && dto.FullName != doctor.FullName)
                doctor.FullName = dto.FullName;
            if (!string.IsNullOrEmpty(dto.PhoneNumber) && dto.PhoneNumber != doctor.PhoneNumber)
                doctor.PhoneNumber = dto.PhoneNumber;
            await _userManager.UpdateAsync(doctor);

            // Cập nhật vai trò từ Doctor sang Manager
            if (!string.IsNullOrEmpty(dto.NewRole) && dto.NewRole != "Manager")
                throw new Exception("Vai trò mới phải là Manager");
            await _userManager.AddToRoleAsync(doctor, "Manager");
            await _userManager.RemoveFromRoleAsync(doctor, "Doctor");

            if (doctorEntity != null)
            {
                _dbContext.Set<User>().Remove(doctorEntity);
                await _dbContext.SaveChangesAsync();
            }

            await _emailService.SendAsync(doctor.Email, "Cập nhật vai trò", "Vai trò của bạn đã được Admin cập nhật thành Manager. Chào mừng bạn!");
            return new
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                Email = doctor.Email,
                PhoneNumber = doctor.PhoneNumber,
                UserName = doctor.UserName,
                Role = "Manager",
                Gender = doctor.Gender,
                Status = doctor.Status,
                DateOfBirth = doctor.DateOfBirth
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
            if (dto.IsActive.HasValue)
                manager.Status = dto.IsActive.Value ? Status.Active : Status.Inactive;

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
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                IsActive = user.Status == Status.Active
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

        public async Task<DashboardDto> GetDashboardStatsAsync()
        {
            var doctorUsers = await _userManager.GetUsersInRoleAsync("Doctor");
            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
            var patientUsers = await _dbContext.Patients.CountAsync();

            var totalAppointments = await _dbContext.Appointments.CountAsync();
            var pendingAppointments = await _dbContext.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending);
            var completedAppointments = await _dbContext.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed);
            var cancelledAppointments = await _dbContext.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled);
            //var totalRevenue = await _dbContext.Appointments
            //    .Where(a => a.Status == AppointmentStatus.Completed)
            //    .SumAsync(a => a.Price ?? 0);

            var stats = new DashboardDto
            {
                TotalDoctors = doctorUsers.Count,
                TotalManagers = managerUsers.Count,
                TotalPatients = patientUsers,
                TotalAppointments = totalAppointments,
                PendingAppointments = pendingAppointments,
                CompletedAppointments = completedAppointments,
                CancelledAppointments = cancelledAppointments,
                //TotalRevenue = totalRevenue
            };

            return stats;
        }
    }
}
