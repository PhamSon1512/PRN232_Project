using MediAppointment.Application.DTOs;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Manager")]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _managerService;

        public ManagerController(IManagerService managerService)
        {
            _managerService = managerService;
        }

        [HttpGet("GetAllDoctors")]
        public async Task<IActionResult> GetAllDoctors([FromQuery] string text = "", [FromQuery] string department = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            try
            {
                var doctors = await _managerService.GetAllDoctorsAsync(text, department, page, pageSize);
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDoctor([FromBody] DoctorCreateDto dto)
        {
            try
            {
                var doctor = await _managerService.CreateDoctorAsync(dto);
                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpDelete("{doctorId:guid}")]
        public async Task<IActionResult> DeleteDoctor(Guid doctorId)
        {
            try
            {
                await _managerService.DeleteDoctorAsync(doctorId);
                return Ok(new { message = "Doctor deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }
    }
}
