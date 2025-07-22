using MediAppointment.Application.DTOs.AppointmentDTOs;
using MediAppointment.Application.DTOs.BookingDoctorDTOs;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediAppointment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        protected readonly IAppointmentService _appointmentService;
        private readonly IAppointmentBookingDoctor _bookingDoctorService;

        public AppointmentController(IAppointmentService appointmentService, IAppointmentBookingDoctor bookingDoctorService)
        {
            _appointmentService = appointmentService;
            _bookingDoctorService = bookingDoctorService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateAppoinment([FromBody] CreateAppointmentRequest request)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value
              ?? throw new Exception("User ID claim is missing"));


            await _appointmentService.CreateAppointment(userId, request);

            return Ok();
        }
        [HttpGet("MyAppointment")]
        public async Task<IActionResult> MyAppointment()
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value
              ?? throw new Exception("User ID claim is missing"));
            var list = await _appointmentService.ListAppointmentByUser(userId);

            return Ok(list);
        }
        [HttpGet("MyAppointment/{AppointmentId:guid}")]
        public async Task<IActionResult> AppointmentDetail(Guid AppointmentId)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value
              ?? throw new Exception("User ID claim is missing"));


            var Appoinment = await _appointmentService.AppointmentDetailById(AppointmentId);

            return Ok(Appoinment);
        }
        [HttpGet("CancelAppoint/{AppointmentId:guid}")]
        public async Task<IActionResult> CancelAppointment(Guid AppointmentId)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value
              ?? throw new Exception("User ID claim is missing"));

            await _appointmentService.CancelById(AppointmentId);
            return Ok("Cancel is Success");
        }
        [HttpPost("GetAppointmentExsit")]
        public async Task<IActionResult> getAppointmentExsit(GetTimeSlotExistDTO request)
        {
            var kq=await _appointmentService.GetTimeSlotExsit(request);
            return Ok(kq);
        }

        [HttpGet("Departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _appointmentService.GetDepartments();
            return Ok(departments);
        }

        [HttpPost("GetAvailableTimeSlots")]
        public async Task<IActionResult> GetAvailableTimeSlots(GetTimeSlotExistDTO request)
        {
            var availableSlots = await _appointmentService.GetAvailableTimeSlotsForBooking(request);
            return Ok(availableSlots);
        }

        [HttpPost("BookAppointment")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return BadRequest(new { error = "User ID claim is missing. Please login again." });
                }

                var userId = Guid.Parse(userIdClaim);
                await _appointmentService.BookAppointment(userId, request);
                
                return Ok(new { message = "Đặt lịch thành công!", success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, success = false });
            }
        }

        [HttpPost("BookAppointmentWithDoctor")]
        public async Task<IActionResult> BookAppointmentWithDoctor([FromBody] BookingDoctorCreate request)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return BadRequest(new { error = "User ID claim is missing. Please login again." });
                }

                request.PatientId = Guid.Parse(userIdClaim);
                await _bookingDoctorService.CreateAsync(request);

                return Ok(new { message = "Đặt lịch hẹn với bác sĩ thành công!", success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, success = false });
            }
        }

        [HttpDelete("CancelBookingDoctor/{appointmentId:guid}")]
        public async Task<IActionResult> CancelBookingDoctor(Guid appointmentId)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID claim is missing.");

            Guid patientId = Guid.Parse(userIdClaim);

            try
            {
                await _bookingDoctorService.CancelAsync(appointmentId, patientId);
                return Ok(new { message = "Hủy lịch thành công", success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, success = false });
            }
        }

        [HttpGet("MyDoctorBookings")]
        public async Task<IActionResult> MyDoctorBookings()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID claim is missing.");

            Guid patientUserId = Guid.Parse(userIdClaim);

            try
            {
                var bookings = await _bookingDoctorService.GetByPatientAsync(patientUserId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
