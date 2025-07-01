using MediAppointment.Application.DTOs.AppointmentDTOs;
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
        public AppointmentController(IAppointmentService appointmentService) {
            _appointmentService = appointmentService;
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


            var Appoinment = await _appointmentService.AppointmentDetailById(userId);

            return Ok(Appoinment);
        }
        [HttpGet("CancelAppoint/{AppointmentId:guid}")]
        public async Task<IActionResult> CancelAppointment(Guid AppointmentId)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value
              ?? throw new Exception("User ID claim is missing"));

            await _appointmentService.CancelById(userId);
            return Ok("Cancel is Success");
        }
        [HttpPost("GetAppointmentExsit")]
        public async Task<IActionResult> getAppointmentExsit(GetTimeSlotExistDTO request)
        {
            var kq=await _appointmentService.GetTimeSlotExsit(request);
            return Ok(kq);
        }
    }
}
