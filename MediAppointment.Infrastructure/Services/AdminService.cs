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

namespace MediAppointment.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<UserIdentity> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public AdminService(UserManager<UserIdentity> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Quản lý Manager
        public async Task<Guid> CreateManagerAsync(string email, string fullName, string phoneNumber, string password)
        {
            var manager = new UserIdentity
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                FullName = fullName,
                PhoneNumber = phoneNumber
            };
            var result = await _userManager.CreateAsync(manager, password);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
            await _userManager.AddToRoleAsync(manager, "Manager");
            return manager.Id;
        }

        public async Task UpdateManagerRoleAsync(Guid managerId, string newRole)
        {
            var manager = await _userManager.FindByIdAsync(managerId.ToString());
            if (manager == null) throw new Exception("Manager not found");
            var currentRoles = await _userManager.GetRolesAsync(manager);
            await _userManager.RemoveFromRolesAsync(manager, currentRoles);
            await _userManager.AddToRoleAsync(manager, newRole);
        }

        public async Task UpdateManagerStatusAsync(Guid managerId, Status status)
        {
            var manager = await _userManager.FindByIdAsync(managerId.ToString());
            if (manager == null) throw new Exception("Manager not found");
            // Giả sử bạn có property Status trong UserIdentity hoặc lưu dưới dạng claim
            // Ở đây sẽ cần mở rộng UserIdentity nếu chưa có property Status
            // manager.Status = status;
            // await _userManager.UpdateAsync(manager);
            // Nếu dùng claim:
            await _userManager.RemoveClaimAsync(manager, new System.Security.Claims.Claim("Status", "Active"));
            await _userManager.AddClaimAsync(manager, new System.Security.Claims.Claim("Status", status.ToString()));
        }

        public async Task UpdateUserStatusAsync(Guid userId, Status status)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new Exception("User not found");
            // Update status claim
            var claims = await _userManager.GetClaimsAsync(user);
            var oldStatus = claims.FirstOrDefault(c => c.Type == "Status");
            if (oldStatus != null)
                await _userManager.RemoveClaimAsync(user, oldStatus);
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Status", status.ToString()));
        }

        public async Task UpdateUserRoleAsync(Guid userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new Exception("User not found");
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);
        }
    }
}
