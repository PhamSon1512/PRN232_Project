using MediAppointment.Application.DTOs;
using MediAppointment.Domain.Enums;
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

        // Quản lý Manager
        [HttpPost("manager")]
        public async Task<IActionResult> CreateManager([FromBody] CreateManagerRequest request)
        {
            var id = await _adminService.CreateManagerAsync(request.Email, request.FullName, request.PhoneNumber, request.Password);
            return Ok(id);
        }

        [HttpPut("manager/{id}/role")]
        public async Task<IActionResult> UpdateManagerRole(string id, [FromQuery] string newRole)
        {
            if (!Guid.TryParse(id, out var guid)) return BadRequest("Invalid Guid format");
            await _adminService.UpdateManagerRoleAsync(guid, newRole);
            return Ok();
        }

        [HttpPut("manager/{id}/status")]
        public async Task<IActionResult> UpdateManagerStatus(string id, [FromQuery] Status status)
        {
            if (!Guid.TryParse(id, out var guid)) return BadRequest("Invalid Guid format");
            await _adminService.UpdateManagerStatusAsync(guid, status);
            return Ok();
        }

    public class CreateManagerRequest
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Password { get; set; }
    }
    }
}
