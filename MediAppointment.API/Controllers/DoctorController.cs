using MediAppointment.Application.DTOs;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IAppointmentService _appointmentService;
        private readonly IIdentityService _identityService;

        public DoctorController(IProfileService profileService, IIdentityService identityService)
        {
            _profileService = profileService;
            _identityService = identityService;
        }

        [HttpGet("profile/{userIdentityId:guid}")]
        //public async Task<IActionResult> Profile(Guid userIdentityId)
        //{
        //    var doctor = await _profileService.GetProfileByIdAsync(userIdentityId);
        //    if (doctor == null) return NotFound();

        //    return Ok(doctor);
        //}
        public async Task<IActionResult> Profile(Guid userIdentityId)
        {
            var doctor = await _identityService.GetDoctorByIdAsync(userIdentityId);
            if (doctor == null) return NotFound();

            return Ok(doctor);
        }

        [HttpPut("profile")]
        //public async Task<IActionResult> UpdateProfile([FromBody] DoctorUpdateDto dto)
        //{
        //    try
        //    {
        //        var updatedDoctor = await _profileService.UpdateProfileAsync(dto);
        //        return Ok(updatedDoctor);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        public async Task<IActionResult> UpdateProfile([FromBody] DoctorUpdateDto dto)
        {
            try
            {
                var updatedDoctor = _identityService.UpdateDoctorAsync(dto); ;
                return Ok(updatedDoctor);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("appointments/{doctorId:guid}")]
        public async Task<IActionResult> GetAppointmentsAssignedToDoctor(
            Guid doctorId,
            [FromQuery] DateTime? date,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)  
        {
            // Nếu không truyền gì → mặc định: 00:00 hôm nay đến 7 ngày sau
            if (!date.HasValue && (!startDate.HasValue || !endDate.HasValue))
            {
                startDate = DateTime.Today; // 00:00 hôm nay
                endDate = startDate.Value.AddDays(7); // 7 ngày sau
            }

            var appointments = await _appointmentService
                .ListAppointmentsAssignedToDoctor(doctorId, date, startDate, endDate);

            return Ok(appointments);
        }

    }
}