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

        public async Task<IActionResult> UpdateUserStatus(Guid id, Status status)
        {
            await _adminService.UpdateUserStatusAsync(id, status);
            return RedirectToAction("UserManagement");
        }

        public async Task<IActionResult> UpdateUserRole(Guid id, string newRole)
        {
            await _adminService.UpdateUserRoleAsync(id, newRole);
            return RedirectToAction("UserManagement");
        }
    }
}
