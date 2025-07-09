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

        [HttpPost("CreateSchedule")]
        public async Task<IActionResult> CreateSchedule([FromBody] List<DoctorScheduleCreateRequest> requests)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("UserId")?.Value
                  ?? throw new Exception("User ID claim is missing"));

                var doctorScheduleRequests = new List<DoctorScheduleRequest>();
                
                foreach (var request in requests)
                {
                    // For each time slot in the request, create a separate DoctorScheduleRequest
                    foreach (var timeSlotId in request.TimeSlotIds)
                    {
                        doctorScheduleRequests.Add(new DoctorScheduleRequest
                        {
                            RoomId = request.RoomId,
                            DateSchedule = request.Date,
                            Shift = request.Period.ToLower() == "afternoon" // true for afternoon, false for morning
                        });
                    }
                }
                
                await _service.CreateDoctorSchedule(userId, doctorScheduleRequests);
                return Ok(new { success = true, message = "Đăng ký lịch làm việc thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("DeleteSchedule")]
        public async Task<IActionResult> DeleteSchedule([FromBody] DoctorScheduleDeleteRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("UserId")?.Value
                  ?? throw new Exception("User ID claim is missing"));
                
                var deleteRequest = new DeleteDoctorScheduleDTO
                {
                    date = request.Date,
                    Shift = request.Period.ToLower() == "afternoon", // true for afternoon, false for morning
                    RoomId = request.RoomId
                };
                
                await _service.DeleteDoctorSchedule(userId, deleteRequest);
                return Ok(new { success = true, message = "Hủy lịch làm việc thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetSchedule")]
        public async Task<IActionResult> GetSchedule(Guid roomId, int year, int week)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("UserId")?.Value
                  ?? throw new Exception("User ID claim is missing"));

                // Calculate week start and end dates
                var jan1 = new DateTime(year, 1, 1);
                var daysOffset = (int)DayOfWeek.Monday - (int)jan1.DayOfWeek;
                var firstMonday = jan1.AddDays(daysOffset);
                var startOfWeek = firstMonday.AddDays((week - 1) * 7);
                var endOfWeek = startOfWeek.AddDays(6);

                var request = new DoctorScheduleRequestDTO
                {
                    DoctorId = userId,
                    RoomId = roomId,
                    DateStart = startOfWeek,
                    DateEnd = endOfWeek
                };

                var scheduleData = await _service.GetDoctorSchedule(request);
                
                // Transform to expected format - only return registered slots
                var result = new List<object>();
                foreach (var schedule in scheduleData)
                {
                    // Add morning slots if doctor is registered for this day
                    if (!string.IsNullOrEmpty(schedule.RoomMorning))
                    {
                        result.Add(new
                        {
                            Id = Guid.NewGuid(),
                            Date = schedule.Date,
                            Period = "morning",
                            TimeRange = "07:00 - 08:00",
                            IsSelected = true,
                            IsOccupied = true
                        });
                        result.Add(new
                        {
                            Id = Guid.NewGuid(),
                            Date = schedule.Date,
                            Period = "morning",
                            TimeRange = "08:00 - 09:00",
                            IsSelected = true,
                            IsOccupied = true
                        });
                        result.Add(new
                        {
                            Id = Guid.NewGuid(),
                            Date = schedule.Date,
                            Period = "morning",
                            TimeRange = "09:00 - 10:00",
                            IsSelected = true,
                            IsOccupied = true
                        });
                        result.Add(new
                        {
                            Id = Guid.NewGuid(),
                            Date = schedule.Date,
                            Period = "morning",
                            TimeRange = "10:00 - 11:00",
                            IsSelected = true,
                            IsOccupied = true
                        });
                    }

                    // Add afternoon slots if doctor is registered for this day
                    if (!string.IsNullOrEmpty(schedule.RoomAfternoon))
                    {
                        result.Add(new
                        {
                            Id = Guid.NewGuid(),
                            Date = schedule.Date,
                            Period = "afternoon",
                            TimeRange = "13:00 - 14:00",
                            IsSelected = true,
                            IsOccupied = true
                        });
                        result.Add(new
                        {
                            Id = Guid.NewGuid(),
                            Date = schedule.Date,
                            Period = "afternoon",
                            TimeRange = "14:00 - 15:00",
                            IsSelected = true,
                            IsOccupied = true
                        });
                        result.Add(new
                        {
                            Id = Guid.NewGuid(),
                            Date = schedule.Date,
                            Period = "afternoon",
                            TimeRange = "15:00 - 16:00",
                            IsSelected = true,
                            IsOccupied = true
                        });
                        result.Add(new
                        {
                            Id = Guid.NewGuid(),
                            Date = schedule.Date,
                            Period = "afternoon",
                            TimeRange = "16:00 - 17:00",
                            IsSelected = true,
                            IsOccupied = true
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
