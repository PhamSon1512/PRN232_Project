using MediAppointment.Application.DTOs.MedicalRecordDtos;
using System.Security.Claims;
using MediAppointment.Application.Interfaces;
using MediAppointment.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IMedicalRecordService _medicalRecordService;

        public PatientController(IPatientService patientService, IMedicalRecordService medicalRecordService)
        {
            _patientService = patientService;
            _medicalRecordService = medicalRecordService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetPatientWithRecords()
        {
            var userIdString = User.FindFirst(c => c.Type == "UserId")?.Value;
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized(new { message = "UserId không hợp lệ" });
            }

            var result = await _patientService.GetPatientWithRecordsAsync(userId);

            if (result == null)
                return NotFound("Không tìm thấy bệnh nhân.");

            return Ok(result);
        }

        [HttpPost]
        [Route("add-medical-record")]
        public async Task<IActionResult> AddMedicalRecord([FromBody] CreateMedicalRecordDto dto)
        {
            try
            {

                var doctorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (doctorIdStr == null)
                    return Unauthorized("Missing name identifier.");

                Guid doctorId = Guid.Parse(doctorIdStr);

                var id = await _medicalRecordService.CreateMedicalRecordAsync(dto, doctorId);
                return Ok(new { Message = "Medical record created successfully", MedicalRecordId = id });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Internal Server Error",
                    Details = ex.InnerException?.Message ?? ex.Message
                });
            }

        }

    }
}
