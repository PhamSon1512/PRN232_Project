using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.MedicalRecordDtos;
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
        private readonly IRoomTimeSlotService _roomTimeSlotService;
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly IPatientService _patientService;

        public DoctorAppoinmentController(
            IRoomTimeSlotService roomTimeSlotService,
            IPatientService patientService,
            IMedicalRecordService medicalRecordService)
        {
            _roomTimeSlotService = roomTimeSlotService;
            _patientService = patientService;
            _medicalRecordService = medicalRecordService;
        }

  
        [HttpGet("assigned-slots/{doctorId}")]
        public async Task<IActionResult> GetAssignedSlots(Guid doctorId, [FromQuery] int? year, [FromQuery] int? week)
        {
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _medicalRecordService.CreateMedicalRecordAsync(dto);
                return Ok(new { Message = "Medical record created successfully", MedicalRecordId = id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal Server Error", Details = ex.Message });
            }
        }
    }
}