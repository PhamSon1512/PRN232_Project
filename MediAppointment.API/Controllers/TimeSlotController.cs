using MediAppointment.Domain.Interfaces;
using MediAppointment.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediAppointment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSlotController : ControllerBase
    {
        private readonly IGenericRepository<TimeSlot> _timeSlotRepository;

        public TimeSlotController(IGenericRepository<TimeSlot> timeSlotRepository)
        {
            _timeSlotRepository = timeSlotRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetTimeSlots()
        {
            try
            {
                var timeSlots = await _timeSlotRepository.Entities
                    .OrderBy(ts => ts.TimeStart)
                    .ToListAsync();

                var result = timeSlots.Select(ts => new
                {
                    Id = ts.Id,
                    TimeRange = $"{ts.TimeStart:hh\\:mm} - {ts.TimeEnd:hh\\:mm}",
                    TimeStart = ts.TimeStart,
                    TimeEnd = ts.TimeEnd,
                    Period = ts.Shift ? "afternoon" : "morning"
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving time slots: " + ex.Message });
            }
        }
    }
}
