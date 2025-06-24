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
            Guid id = Guid.Parse("497DD1AE-1135-4169-B8EA-ED3485C6C2CB");
            await _service.CreateDoctorSchedule(id, requests);
            return Ok();
        }
        [HttpGet("DeleteRoomTimeSlot/{RoomTimeSlotId:guid}")]
        public async Task<IActionResult> Get(Guid RoomTimeSlotId)
        {
            /*var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out Guid id))
                {
                }*/
            Guid id = Guid.Parse("497DD1AE-1135-4169-B8EA-ED3485C6C2CB");
            await _service.DeleteDoctorSchedule(RoomTimeSlotId);
            return Ok();
        }
    }
}
