using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.BookingDoctorDTOs;
using MediAppointment.Application.DTOs.MedicalRecordDtos;
using MediAppointment.Application.Interfaces;
using MediAppointment.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediAppointment.API.Controllers
{
    [Authorize(Roles = "Doctor")]
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorAppoinmentController : ControllerBase
    {
        private readonly IRoomTimeSlotService _roomTimeSlotService;
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly IPatientService _patientService;
        private readonly IAppointmentBookingDoctor _bookingDoctorService;

        public DoctorAppoinmentController(
            IRoomTimeSlotService roomTimeSlotService,
            IPatientService patientService,
            IMedicalRecordService medicalRecordService,
            IAppointmentBookingDoctor bookingDoctorService)
        {
            _roomTimeSlotService = roomTimeSlotService;
            _patientService = patientService;
            _medicalRecordService = medicalRecordService;
            _bookingDoctorService = bookingDoctorService;
        }


        [HttpGet("assigned-slots")]
        public async Task<IActionResult> GetAssignedSlots([FromQuery] int? year, [FromQuery] int? week)
        {
            var doctorIdClaim = User.FindFirst("UserId");
            if (doctorIdClaim == null)
                return Unauthorized("Missing UserId claim.");

            Guid doctorId = Guid.Parse(doctorIdClaim.Value);

            var slots = await _roomTimeSlotService.GetAssignedSlotsByDoctor(doctorId, year, week);
            return Ok(slots);
        }

        [HttpGet("appointments-by-slot/{roomTimeSlotId:guid}")]
        public async Task<IActionResult> GetRoomTimeSlotDetailWithAppointments(Guid roomTimeSlotId)
        {
            var result = await _roomTimeSlotService.GetDetailWithAppointmentsAsync(roomTimeSlotId);
            if (result == null) return NotFound("Không tìm thấy slot.");
            return Ok(result);
        }

        [HttpGet("patient-detail/{patientId:guid}")]
        public async Task<IActionResult> GetPatientWithRecords(Guid patientId)
        {
            var result = await _patientService.GetPatientWithRecordsAsync(patientId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CreateMedicalRecordDto dto)
        {
            try
            {

                var doctorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (doctorIdStr == null)
                    return Unauthorized("Missing name identifier.");

                Guid doctorId = Guid.Parse(doctorIdStr);

                var id = await _medicalRecordService.CreateMedicalRecordAsync(dto, doctorId);
                return Ok(new { Message = "Medical record created successfully", MedicalRecordId = id });

            }catch(Exception ex)
{
                return StatusCode(500, new
                {
                    Message = "Internal Server Error",
                    Details = ex.InnerException?.Message ?? ex.Message
                });
            }

        }

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetBookingsByDoctor()
        {
            var doctorIdClaim = User.FindFirst("UserId");
            if (doctorIdClaim == null)
                return Unauthorized("Missing UserId claim.");

            Guid doctorId = Guid.Parse(doctorIdClaim.Value);

            var bookings = await _bookingDoctorService.GetByDoctorAsync(doctorId);
            return Ok(bookings);
        }


        [HttpPost("update-booking-status/{appointmentId:guid}")]
        public async Task<IActionResult> UpdateBookingStatus(Guid appointmentId, [FromBody] BookingDoctorStatusUpdate request)
        {
            var doctorIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(doctorIdClaim))
                return Unauthorized("Missing UserId claim.");

            Guid doctorId = Guid.Parse(doctorIdClaim);

            try
            {
                await _bookingDoctorService.UpdateStatusAsync(appointmentId, doctorId, request);
                return Ok(new { Message = "Cập nhật trạng thái lịch hẹn thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Cập nhật trạng thái lịch hẹn thất bại.", Details = ex.Message });
            }
        }

    }
}