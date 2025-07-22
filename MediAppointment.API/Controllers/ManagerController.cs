using System.Security.Claims;
using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.ManagerDTOs;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager")]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _managerService;

        public ManagerController(IManagerService managerService)
        {
            _managerService = managerService;
        }

        [HttpGet("GetAllDoctors")]  
        [AllowAnonymous]
        public async Task<IActionResult> GetAllDoctors([FromQuery] string text = "", /*[FromQuery] string department = ""*/ [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            try
            {
                var doctors = await _managerService.GetAllDoctorsAsync(text, /*department,*/ page, pageSize);
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet("doctors/{doctorId:guid}")]
        public async Task<IActionResult> GetDoctorById(Guid doctorId)
        {
            try
            {
                var doctor = await _managerService.GetDoctorByIdAsync(doctorId);
                if (doctor == null)
                    return NotFound(new { Message = "Doctor not found." });
                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetManagerProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var managerId)) return Unauthorized("Invalid admin token");

            try
            {
                var profile = await _managerService.GetManagerProfileAsync(managerId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpPut("UpdateManagerProfile")]
        public async Task<IActionResult> UpdateManagerProfile([FromBody] ManagerUpdateProfileDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var managerId)) return Unauthorized("Invalid manager token");

            try
            {
                var updated = await _managerService.UpdateManagerProfileAsync(dto);
                if (updated)
                {
                    return Ok(new { Message = "Cập nhật hồ sơ Manager thành công." });
                }
                return BadRequest(new { Message = "Cập nhật hồ sơ Manager thất bại." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDoctor([FromBody] DoctorCreateDto dto)
        {
            var managerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(managerIdClaim, out Guid managerId))
                return Unauthorized(new { Message = "Invalid or missing UserId in JWT token." });

            var doctorId = await _managerService.CreateDoctorAsync(dto);
            return Ok(new { DoctorId = doctorId, Message = "Doctor created successfully." });
        }

        [HttpPut("update/{doctorId:guid}")]
        public async Task<IActionResult> UpdateDoctor(Guid doctorId, [FromBody] ManagerDoctorUpdateDTO dto)
        {
            var managerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(managerIdClaim, out Guid managerId))
                return Unauthorized(new { Message = "Invalid or missing UserId in JWT token." });

            if (dto == null)
                return BadRequest(new { Message = "Invalid update data." });

            var updatedDoctor = await _managerService.ManagerUpdateDoctorAsync(doctorId, dto);
            return Ok(new { DoctorId = updatedDoctor.Id, Message = "Doctor updated successfully." });
        }

        [HttpDelete("{doctorId:guid}")]
        public async Task<IActionResult> DeleteDoctor(Guid doctorId)
        {
            var managerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(managerIdClaim, out Guid managerId))
                return Unauthorized(new { Message = "Invalid or missing UserId in JWT token." });

            await _managerService.DeleteDoctorAsync(doctorId);
            return Ok(new { Message = "Doctor deleted successfully." });
        }

        [HttpGet("WeeklySchedule")]
        public async Task<IActionResult> GetWeeklySchedule([FromQuery] Guid? departmentId, [FromQuery] Guid? roomId, [FromQuery] Guid? doctorId, [FromQuery] int year, [FromQuery] int week)
        {
            try
            {
                var schedule = await _managerService.GetWeeklyScheduleAsync(departmentId, roomId, doctorId, year, week);
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"Lỗi khi lấy lịch hàng tuần: {ex.Message}" });
            }
        }
    }
}
