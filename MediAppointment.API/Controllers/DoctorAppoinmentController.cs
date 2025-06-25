using MediAppointment.Application.DTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorAppoinmentController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IAppointmentService _appointmentService;
        private readonly IRoomTimeSlotService _roomTimeSlotService;

        public DoctorAppoinmentController(IProfileService profileService, IAppointmentService appointmentService, IRoomTimeSlotService roomTimeSlotService)
        {
            _profileService = profileService;
            _appointmentService = appointmentService;
            _roomTimeSlotService = roomTimeSlotService;
        }

        [HttpGet("profile/{userIdentityId:guid}")]
        public async Task<IActionResult> Profile(Guid userIdentityId)
        {
            var doctor = await _profileService.GetProfileByIdAsync(userIdentityId);
            if (doctor == null) return NotFound();

            return Ok(doctor);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] DoctorUpdateDto dto)
        {
            try
            {
                var updatedDoctor = await _profileService.UpdateProfileAsync(dto);
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


        [HttpGet("assigned-slots/{doctorId}")]
        public async Task<IActionResult> GetAssignedSlots(Guid doctorId, [FromQuery] int? year, [FromQuery] int? week)
        {
            var slots = await _roomTimeSlotService.GetAssignedSlotsByDoctor(doctorId, year, week);
            return Ok(slots);
        }

        [HttpGet("appointments-by-slot/{roomTimeSlotId:guid}")]
        public async Task<IActionResult> GetSlotDetailWithAppointments(Guid roomTimeSlotId)
        {
            var result = await _roomTimeSlotService.GetDetailWithAppointmentsAsync(roomTimeSlotId);
            if (result == null) return NotFound("Không tìm thấy slot.");
            return Ok(result);
        }

    }
}