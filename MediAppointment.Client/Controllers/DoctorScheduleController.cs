using MediAppointment.Client.Models.DoctorSchedule;
using MediAppointment.Client.Models.Appointment;
using AdminModels = MediAppointment.Client.Models.Admin;
using MediAppointment.Client.Services;
using MediAppointment.Client.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.Client.Controllers
{
    [RequireDoctor]
    public class DoctorScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        private readonly IAuthService _authService;
        private readonly IDepartmentService _departmentService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly ILogger<DoctorScheduleController> _logger;

        public DoctorScheduleController(IScheduleService scheduleService, IAuthService authService, IDepartmentService departmentService, ITimeSlotService timeSlotService, ILogger<DoctorScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _authService = authService;
            _departmentService = departmentService;
            _timeSlotService = timeSlotService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Manage(Guid? departmentId, Guid? roomId, int? year, int? week)
        {
            var model = new DoctorScheduleManagementViewModel
            {
                DepartmentId = departmentId ?? Guid.Empty,
                RoomId = roomId ?? Guid.Empty,
                Year = year ?? DateTime.Now.Year,
                Week = week ?? GetWeekOfYear(DateTime.Now),
                AvailableYears = Enumerable.Range(DateTime.Now.Year, 2).ToList(),
                AvailableWeeks = Enumerable.Range(1, 53).ToList()
            };

            // Load departments and rooms
            await LoadDepartmentsAndRooms(model);

            // If no specific filters applied, use first room from the list for initial load
            if ((!departmentId.HasValue || departmentId == Guid.Empty) && 
                (!roomId.HasValue || roomId == Guid.Empty) && 
                model.Departments.Any() && model.Rooms.Any())
            {
                model.DepartmentId = model.Departments.First().Id;
                model.RoomId = model.Rooms.First().Id;
                await LoadWeeklySchedule(model);
            }
            // Load schedule if filters are applied
            else if (departmentId.HasValue && departmentId != Guid.Empty && 
                     roomId.HasValue && roomId != Guid.Empty && 
                     year.HasValue && week.HasValue)
            {
                await LoadWeeklySchedule(model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] List<ScheduleCreateRequest> requests)
        {
            try
            {
                if (requests == null || !requests.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một slot thời gian" });
                }

                // Validate all requests
                foreach (var request in requests)
                {
                    if (request.RoomId == Guid.Empty)
                    {
                        return Json(new { success = false, message = "Phòng không hợp lệ" });
                    }
                    if (request.Date == DateTime.MinValue)
                    {
                        return Json(new { success = false, message = "Ngày không hợp lệ" });
                    }
                    if (string.IsNullOrEmpty(request.Period))
                    {
                        return Json(new { success = false, message = "Ca làm việc không hợp lệ" });
                    }
                    if (!request.TimeSlotIds.Any())
                    {
                        return Json(new { success = false, message = "Vui lòng chọn khung giờ" });
                    }
                }

                // Transform to API format and call service
                var result = await _scheduleService.CreateDoctorScheduleAsync(requests);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Đăng ký lịch làm việc thành công!" });
                }

                return Json(new { success = false, message = result.ErrorMessage ?? "Đăng ký lịch thất bại" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSchedule([FromBody] ScheduleDeleteRequest request)
        {
            try
            {
                if (request.RoomId == Guid.Empty || request.Date == DateTime.MinValue || string.IsNullOrEmpty(request.Period))
                {
                    return Json(new { success = false, message = "Thông tin xóa lịch không hợp lệ" });
                }

                var result = await _scheduleService.DeleteDoctorScheduleAsync(request);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Xóa lịch làm việc thành công!" });
                }

                return Json(new { success = false, message = result.ErrorMessage ?? "Xóa lịch thất bại" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkCreateSchedule([FromBody] BulkScheduleRequest request)
        {
            try
            {
                if (request?.Schedules == null || !request.Schedules.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một ca làm việc" });
                }

                var results = new List<object>();
                var successCount = 0;
                var errorCount = 0;

                foreach (var schedule in request.Schedules)
                {
                    var createRequest = new List<ScheduleCreateRequest> { schedule };
                    var result = await _scheduleService.CreateDoctorScheduleAsync(createRequest);
                    
                    results.Add(new
                    {
                        date = schedule.Date.ToString("dd/MM/yyyy"),
                        period = schedule.Period,
                        success = result.Success,
                        message = result.Success ? "Thành công" : result.ErrorMessage
                    });

                    if (result.Success)
                        successCount++;
                    else
                        errorCount++;
                }

                var message = $"Đã đăng ký {successCount} ca thành công";
                if (errorCount > 0)
                    message += $", {errorCount} ca thất bại";

                return Json(new { 
                    success = errorCount == 0, 
                    message = message,
                    details = results,
                    successCount = successCount,
                    errorCount = errorCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk create schedule");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkDeleteSchedule([FromBody] BulkDeleteRequest request)
        {
            try
            {
                if (request?.Schedules == null || !request.Schedules.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một ca để hủy" });
                }

                var results = new List<object>();
                var successCount = 0;
                var errorCount = 0;

                foreach (var schedule in request.Schedules)
                {
                    var result = await _scheduleService.DeleteDoctorScheduleAsync(schedule);
                    
                    results.Add(new
                    {
                        date = schedule.Date.ToString("dd/MM/yyyy"),
                        period = schedule.Period,
                        success = result.Success,
                        message = result.Success ? "Thành công" : result.ErrorMessage
                    });

                    if (result.Success)
                        successCount++;
                    else
                        errorCount++;
                }

                var message = $"Đã hủy {successCount} ca thành công";
                if (errorCount > 0)
                    message += $", {errorCount} ca thất bại";

                return Json(new { 
                    success = errorCount == 0, 
                    message = message,
                    details = results,
                    successCount = successCount,
                    errorCount = errorCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk delete schedule");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRoomsByDepartment(Guid departmentId)
        {
            try
            {
                if (departmentId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Department ID không hợp lệ" });
                }

                var result = await _departmentService.GetRoomsByDepartmentAsync(departmentId);
                
                if (result.Success && result.Data != null)
                {
                    var rooms = result.Data.Select(r => new 
                    { 
                        id = r.Id, 
                        name = r.Name,
                        departmentId = r.DepartmentId
                    }).ToList();
                    
                    return Json(new { success = true, data = rooms });
                }

                return Json(new { success = false, message = "Không tìm thấy phòng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTimeSlots()
        {
            try
            {
                var result = await _timeSlotService.GetTimeSlotsAsync();
                
                if (result.Success && result.Data != null)
                {
                    var timeSlots = result.Data.Select(ts => new 
                    { 
                        id = ts.Id, 
                        timeRange = ts.TimeRange,
                        period = ts.Period // Use the Period property from the API response
                    }).ToList();
                    
                    return Json(new { success = true, data = timeSlots });
                }

                return Json(new { success = false, message = "Không tìm thấy khung giờ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWeeklyScheduleDetails(Guid roomId, int year, int week)
        {
            try
            {
                if (roomId == Guid.Empty || year <= 0 || week <= 0 || week > 53)
                {
                    return Json(new { success = false, message = "Thông tin lọc không hợp lệ" });
                }

                var result = await _scheduleService.GetDoctorScheduleAsync(roomId, year, week);
                
                if (result.Success && result.Data != null)
                {
                    // Calculate week dates
                    var jan1 = new DateTime(year, 1, 1);
                    var daysOffset = (int)DayOfWeek.Monday - (int)jan1.DayOfWeek;
                    var firstMonday = jan1.AddDays(daysOffset);
                    var startOfWeek = firstMonday.AddDays((week - 1) * 7);
                    var weekDates = Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i)).ToList();

                    var weeklyData = new Dictionary<string, object>();
                    foreach (var date in weekDates)
                    {
                        var dailySlots = result.Data.Where(s => s.Date.Date == date.Date)
                                                   .GroupBy(s => s.Period)
                                                   .ToDictionary(g => g.Key, g => g.ToList());
                        
                        weeklyData[date.ToString("yyyy-MM-dd")] = new
                        {
                            date = date.ToString("dd/MM/yyyy"),
                            dayOfWeek = date.ToString("dddd", new System.Globalization.CultureInfo("vi-VN")),
                            morning = dailySlots.ContainsKey("morning") ? dailySlots["morning"] : new List<ScheduleSlot>(),
                            afternoon = dailySlots.ContainsKey("afternoon") ? dailySlots["afternoon"] : new List<ScheduleSlot>()
                        };
                    }

                    return Json(new { success = true, data = weeklyData });
                }

                return Json(new { success = false, message = "Không tìm thấy lịch làm việc" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekly schedule details");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ValidateScheduleConflict(Guid roomId, DateTime date, string period)
        {
            try
            {
                if (roomId == Guid.Empty || date == DateTime.MinValue || string.IsNullOrEmpty(period))
                {
                    return Json(new { success = false, message = "Thông tin không hợp lệ" });
                }

                // Check if there's already a schedule for this room, date, and period
                var existingSchedule = await _scheduleService.GetDoctorScheduleAsync(roomId, date.Year, GetWeekOfYear(date));
                
                if (existingSchedule.Success && existingSchedule.Data != null)
                {
                    var conflict = existingSchedule.Data.Any(s => s.Date.Date == date.Date && s.Period == period);
                    
                    return Json(new { 
                        success = true, 
                        hasConflict = conflict,
                        message = conflict ? "Đã có lịch làm việc cho ca này" : "Có thể đăng ký ca này"
                    });
                }

                return Json(new { success = true, hasConflict = false, message = "Có thể đăng ký ca này" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating schedule conflict");
                return Json(new { success = false, message = "Có lỗi xảy ra khi kiểm tra lịch" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetScheduleStatistics(Guid roomId, int year, int week)
        {
            try
            {
                if (roomId == Guid.Empty || year <= 0 || week <= 0)
                {
                    return Json(new { success = false, message = "Thông tin không hợp lệ" });
                }

                var result = await _scheduleService.GetDoctorScheduleAsync(roomId, year, week);
                
                if (result.Success && result.Data != null)
                {
                    var stats = new
                    {
                        totalSlots = result.Data.Count,
                        morningSlots = result.Data.Count(s => s.Period == "morning"),
                        afternoonSlots = result.Data.Count(s => s.Period == "afternoon"),
                        totalDays = result.Data.GroupBy(s => s.Date.Date).Count(),
                        registeredDays = result.Data.GroupBy(s => s.Date.Date)
                                                   .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), 
                                                                g => new { 
                                                                    morning = g.Any(s => s.Period == "morning"),
                                                                    afternoon = g.Any(s => s.Period == "afternoon")
                                                                })
                    };
                    
                    return Json(new { success = true, data = stats });
                }

                return Json(new { success = false, message = "Không thể tải thống kê lịch làm việc" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule statistics");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải thống kê" });
            }
        }

        private async Task LoadDepartmentsAndRooms(DoctorScheduleManagementViewModel model)
        {
            try
            {
                _logger.LogInformation("Starting LoadDepartmentsAndRooms");
                
                // Load departments
                var departmentsResult = await _departmentService.GetDepartmentsAsync();
                _logger.LogInformation("Departments result: Success={Success}, Count={Count}", 
                    departmentsResult.Success, departmentsResult.Data?.Count ?? 0);
                
                if (departmentsResult.Success && departmentsResult.Data != null)
                {
                    model.Departments = departmentsResult.Data;

                    // Load rooms for all departments
                    var allRooms = new List<RoomOption>();
                    foreach (var department in model.Departments)
                    {
                        _logger.LogInformation("Loading rooms for department {DepartmentId} - {DepartmentName}", 
                            department.Id, department.Name);
                        
                        var roomsResult = await _departmentService.GetRoomsByDepartmentAsync(department.Id);
                        _logger.LogInformation("Rooms result for department {DepartmentId}: Success={Success}, Count={Count}", 
                            department.Id, roomsResult.Success, roomsResult.Data?.Count ?? 0);
                        
                        if (roomsResult.Success && roomsResult.Data != null)
                        {
                            var mappedRooms = roomsResult.Data.Select(r => new RoomOption
                            {
                                Id = r.Id,
                                Name = r.Name,
                                DepartmentId = r.DepartmentId
                            }).ToList();
                            allRooms.AddRange(mappedRooms);
                        }
                    }
                    model.Rooms = allRooms;
                    _logger.LogInformation("Total rooms loaded: {Count}", allRooms.Count);
                }
                else
                {
                    _logger.LogWarning("Failed to load departments: {ErrorMessage}", departmentsResult.ErrorMessage);
                    // Return empty lists if API fails
                    model.Departments = new List<DepartmentOption>();
                    model.Rooms = new List<RoomOption>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoadDepartmentsAndRooms");
                model.Departments = new List<DepartmentOption>();
                model.Rooms = new List<RoomOption>();
            }
        }

        private async Task LoadWeeklySchedule(DoctorScheduleManagementViewModel model)
        {
            try
            {
                // Get doctor's existing schedule for the selected room and week
                var result = await _scheduleService.GetDoctorScheduleAsync(model.RoomId, model.Year, model.Week);
                
                if (result.Success && result.Data != null)
                {
                    // Calculate week dates
                    var jan1 = new DateTime(model.Year, 1, 1);
                    var daysOffset = (int)DayOfWeek.Monday - (int)jan1.DayOfWeek;
                    var firstMonday = jan1.AddDays(daysOffset);
                    var startOfWeek = firstMonday.AddDays((model.Week - 1) * 7);
                    var weekDates = Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i)).ToList();

                    // Group schedule data by date
                    foreach (var date in weekDates)
                    {
                        var dailySlots = result.Data.Where(s => s.Date.Date == date.Date).ToList();
                        model.WeeklySchedule[date.Date] = dailySlots;
                    }
                }
                else
                {
                    // Return empty schedule if API fails
                    CreateEmptyWeekSchedule(model);
                }
            }
            catch (Exception)
            {
                // Return empty schedule on error
                CreateEmptyWeekSchedule(model);
            }
        }

        private void CreateEmptyWeekSchedule(DoctorScheduleManagementViewModel model)
        {
            // Calculate week dates
            var jan1 = new DateTime(model.Year, 1, 1);
            var daysOffset = (int)DayOfWeek.Monday - (int)jan1.DayOfWeek;
            var firstMonday = jan1.AddDays(daysOffset);
            var startOfWeek = firstMonday.AddDays((model.Week - 1) * 7);
            var weekDates = Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i)).ToList();

            // Create empty schedule - no slots means "register" buttons will be shown
            foreach (var date in weekDates)
            {
                model.WeeklySchedule[date.Date] = new List<ScheduleSlot>();
            }
        }

        private static int GetWeekOfYear(DateTime date)
        {
            var jan1 = new DateTime(date.Year, 1, 1);
            var daysOffset = (int)DayOfWeek.Monday - (int)jan1.DayOfWeek;
            var firstMonday = jan1.AddDays(daysOffset);
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }
    }
}
