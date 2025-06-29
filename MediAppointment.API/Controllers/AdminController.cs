using MediAppointment.Application.DTOs.AdminDTOs;
using MediAppointment.Application.DTOs.UserDTOs;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAdmins()
        {
            var admins = await _adminService.GetAllAdminsAsync();
            return Ok(admins);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdminById(string id)
        {
            if (!Guid.TryParse(id, out var guid)) return BadRequest("Invalid Guid format");
            var admin = await _adminService.GetAdminByIdAsync(guid);
            if (admin == null) return NotFound();
            return Ok(admin);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] AdminDTO adminDto)
        {
            await _adminService.CreateAdminAsync(adminDto);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAdmin([FromBody] AdminDTO adminDto)
        {
            await _adminService.UpdateAdminAsync(adminDto);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            if (!Guid.TryParse(id, out var guid)) return BadRequest("Invalid Guid format");
            await _adminService.DeleteAdminAsync(guid);
            return Ok();
        }

        // Quản lý người dùng
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            if (!Guid.TryParse(id, out var guid)) return BadRequest("Invalid Guid format");
            var user = await _adminService.GetUserByIdAsync(guid);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> SetUserStatus(string id, [FromQuery] bool isActive)
        {
            if (!Guid.TryParse(id, out var guid)) return BadRequest("Invalid Guid format");
            await _adminService.SetUserStatusAsync(guid, isActive);
            return Ok();
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> ChangeUserRole(string id, [FromQuery] string newRole)
        {
            if (!Guid.TryParse(id, out var guid)) return BadRequest("Invalid Guid format");
            await _adminService.ChangeUserRoleAsync(guid, newRole);
            return Ok();
        }
    }
}
