using MediAppointment.Application.DTOs.DoctorScheduleDTOs;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorScheduleController : ControllerBase
    {
        protected IDoctorScheduleService _service;
        public DoctorScheduleController(IDoctorScheduleService service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task<IActionResult> Post(List<DoctorScheduleRequest> requests) {

            var userId = Guid.Parse(User.FindFirst("UserId")?.Value
              ?? throw new Exception("User ID claim is missing"));
            await _service.CreateDoctorSchedule(userId, requests);
            return Ok();
        }
        [HttpDelete]
        public async Task<IActionResult> Get(DeleteDoctorScheduleDTO request)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value
              ?? throw new Exception("User ID claim is missing"));
            await _service.DeleteDoctorSchedule(userId,request);
            return Ok();
        }
        [HttpPost("GetDoctorSchedule")]
        public async Task<IActionResult> GetDoctorShcedule(DoctorScheduleRequestDTO request)
        {
            var x = await _service.GetDoctorSchedule(request);
            return Ok(x);
        }
    }
}
