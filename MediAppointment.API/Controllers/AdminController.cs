using MediAppointment.Application.DTOs;
using MediAppointment.Domain.Enums;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediAppointment.Application.DTOs.ManagerDTOs;

namespace MediAppointment.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("GetAllDoctorsAndManagers")]
        public async Task<IActionResult> GetAllDoctorsAndManagers([FromQuery] string text = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var adminId)) return Unauthorized("Invalid admin token");

                var result = await _adminService.GetAllDoctorsAndManagersAsync(text, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet("AdminProfile")]
        public async Task<IActionResult> GetAdminProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var adminId)) return Unauthorized("Invalid admin token");

            var profile = await _adminService.GetAdminProfileAsync(adminId);
            return Ok(profile);
        }

        [HttpPut("CreateManager")]
        public async Task<IActionResult> CreateDoctorToManager([FromBody] ManagerCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var adminId)) return Unauthorized("Invalid admin token");

            try
            {
                var result = await _adminService.CreateDoctorToManagerAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpPut("UpdateManagerProfile")]
        public async Task<IActionResult> UpdateManagerProfile([FromBody] ManagerUpdateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var adminId)) return Unauthorized("Invalid admin token");

            try
            {
                var result = await _adminService.UpdateManagerProfileAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }
    }
}
