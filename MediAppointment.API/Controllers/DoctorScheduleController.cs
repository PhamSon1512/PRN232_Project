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
            /*var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out Guid id))
                {
                }*/
            Guid id = Guid.Parse("FD09F480-BD97-4CDA-9402-52E343ED6090");
            await _service.CreateDoctorSchedule(id, requests);
            return Ok();
        }
        [HttpDelete]
        public async Task<IActionResult> Get(DeleteDoctorScheduleDTO request)
        {
            /*var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out Guid id))
                {
                }*/
            Guid id = Guid.Parse("FD09F480-BD97-4CDA-9402-52E343ED6090");
            await _service.DeleteDoctorSchedule(id,request);
            return Ok();
        }
    }
}
