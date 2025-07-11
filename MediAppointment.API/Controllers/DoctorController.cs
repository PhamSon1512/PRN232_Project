using System.Security.Claims;
using MediAppointment.Application.DTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IAppointmentService _appointmentService;
        private readonly IIdentityService _identityService;

        public DoctorController(IProfileService profileService, IIdentityService identityService, IAppointmentService appointmentService)
        {
            _profileService = profileService;
            _identityService = identityService;
            _appointmentService = appointmentService;
        }

        [HttpPost("profile")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorProfile([FromBody] Guid doctorId)
        {
            try
            {
                var doctor = await _identityService.GetDoctorByIdAsync(doctorId);   
                if (doctor == null)
                    return NotFound(new { Message = "Doctor not found." });

                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(7);
                var appointments = await _appointmentService.ListAppointmentsAssignedToDoctor(doctorId, null, startDate, endDate);
                //var schedules = appointments.Select(a => new
                //{
                //    Id = a.Id,
                //    Date = a.AppointmentDate,
                //    Shift = a.Time,
                //    IsAvailable = a.Status == "Scheduled"
                //}).ToList();

                var doctorViewModel = new
                {
                    Id = doctor.Id,
                    FullName = doctor.FullName,
                    Gender = doctor.Gender,
                    DateOfBirth = doctor.DateOfBirth,
                    Email = doctor.Email,
                    PhoneNumber = doctor.PhoneNumber,
                    Departments = doctor.Departments,
                    //Schedules = schedules,
                    Status = doctor.Status
                };

                return Ok(doctorViewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet("DoctorProfile")]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (Guid.TryParse(userIdClaim, out Guid id)){}
            //var user = Guid.Parse("B6B013AD-B2B2-481C-88B1-87E3B692A367");
            var doctor = await _identityService.GetDoctorByIdAsync(id);
            if (doctor == null) return NotFound();

            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(7);
            var appointments = await _appointmentService.ListAppointmentsAssignedToDoctor(id, null, startDate, endDate);
            //var schedules = appointments.Select(a => new
            //{
            //    Id = a.Id,
            //    Date = a.AppointmentDate,
            //    Shift = a.Time,
            //    IsAvailable = a.Status == "Scheduled"
            //}).ToList();

            var doctorViewModel = new
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                Gender = doctor.Gender,
                DateOfBirth = doctor.DateOfBirth,
                Email = doctor.Email,
                PhoneNumber = doctor.PhoneNumber,
                Departments = doctor.Departments,
                //Schedules = schedules,
                Status = doctor.Status
            };

            return Ok(doctorViewModel);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] DoctorUpdateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new { Message = "Invalid or missing UserId in JWT token." });

            if (dto == null)
                return BadRequest(new { Message = "Invalid profile data." });

            var updatedDoctor = await _profileService.UpdateProfileAsync(userId, dto);
            return Ok(updatedDoctor);
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
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllDoctors()
        {
            var pagedResult = await _identityService.GetAllDoctorsAsync();

            var doctorViewModels = pagedResult.Items.Select(d => new
            {
                Id = d.Id,
                FullName = d.FullName,
                Gender = d.Gender,
                DateOfBirth = d.DateOfBirth,
                Email = d.Email,
                PhoneNumber = d.PhoneNumber,
                Departments = d.Departments,
                Schedules = new List<object>(),
                Status = d.Status
            }).ToList();

            return Ok(doctorViewModels);
        }
    }
}