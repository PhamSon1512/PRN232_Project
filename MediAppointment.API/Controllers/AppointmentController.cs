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
        public async Task<IActionResult> CreateAppoinment([FromBody]CreateAppointmentRequest request)
        {
            /*var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid id))
            {
            }*/
            Guid id = Guid.Parse("C7D334CA-90BC-4E77-BCE0-4237F5F246F2");

            await _appointmentService.CreateAppointment(id, request);

            return Ok();
        }
        [HttpGet("MyAppointment")]
        public async Task<IActionResult> MyAppointment()
        {
            /*var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid id))
            {
            }*/
            Guid id = Guid.Parse("C7D334CA-90BC-4E77-BCE0-4237F5F246F2");
            var list = await _appointmentService.ListAppointmentByUser(id);

            return Ok(list);
        }
        [HttpGet("MyAppointment/{AppointmentId:guid}")]
        public async Task<IActionResult> AppointmentDetail(Guid AppointmentId)
        {
            /*var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid id))
            {
            }*/
            
            var Appoinment = await _appointmentService.AppointmentDetailById(AppointmentId);

            return Ok(Appoinment);
        }
        [HttpGet("CancelAppoint/{AppointmentId:guid}")]
        public async Task<IActionResult> CancelAppointment(Guid AppointmentId)
        {
            /*var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid id))
            {
            }*/

            await _appointmentService.CancelById(AppointmentId);
            return Ok("Cancel is Success");
        }
    }
}
