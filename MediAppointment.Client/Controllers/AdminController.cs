using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MediAppointment.Client.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [Route("Admin")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [Route("Admin/UserManagement")]
        public async Task<IActionResult> UserManagement()
        {
            var users = await _adminService.GetAllUsersAsync();
            return View("Index", users);
        }

        public async Task<IActionResult> SetUserStatus(Guid id, bool isActive)
        {
            await _adminService.SetUserStatusAsync(id, isActive);
            return RedirectToAction("UserManagement");
        }

        public async Task<IActionResult> ChangeUserRole(Guid id, string newRole)
        {
            await _adminService.ChangeUserRoleAsync(id, newRole);
            return RedirectToAction("UserManagement");
        }
    }
}
