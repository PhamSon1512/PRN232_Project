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
            var currentYear = DateTime.Now.Year;
            var currentWeek = GetWeekOfYear(DateTime.Now);
            
            var model = new DoctorScheduleManagementViewModel
            {
                DepartmentId = departmentId ?? Guid.Empty,
                RoomId = roomId ?? Guid.Empty,
                Year = year ?? currentYear,
                Week = week ?? currentWeek,
                AvailableYears = Enumerable.Range(currentYear, 2).ToList(),
                AvailableWeeks = Enumerable.Range(1, 53).ToList()
            };

            // Load departments and rooms
            await LoadDepartmentsAndRooms(model);

            // Auto-select first department and room if none selected
            if (model.DepartmentId == Guid.Empty && model.Departments.Any())
            {
                model.DepartmentId = model.Departments.First().Id;
            }
            
            if (model.RoomId == Guid.Empty && model.Rooms.Any())
            {
                var departmentRooms = model.Rooms.Where(r => r.DepartmentId == model.DepartmentId).ToList();
                if (departmentRooms.Any())
                {
                    model.RoomId = departmentRooms.First().Id;
                }
            }

            // Always try to load schedule if we have department, room, year, and week
            if (model.DepartmentId != Guid.Empty && model.RoomId != Guid.Empty)
            {
                await LoadWeeklySchedule(model);
            }
            else
            {
                // Create empty schedule if missing required data
                CreateEmptyWeekSchedule(model);
            }

            return View(model);
        }

        // Thêm POST action để xử lý filter form submission
        [HttpPost]
        public async Task<IActionResult> Manage(DoctorScheduleManagementViewModel model)
        {
            try
            {
                // Ensure we have valid year and week
                if (model.Year <= 0)
                    model.Year = DateTime.Now.Year;
                if (model.Week <= 0)
                    model.Week = GetWeekOfYear(DateTime.Now);

                model.AvailableYears = Enumerable.Range(DateTime.Now.Year, 2).ToList();
                model.AvailableWeeks = Enumerable.Range(1, 53).ToList();

                // Load departments and rooms
                await LoadDepartmentsAndRooms(model);
                
                // Validate selections
                if (model.DepartmentId != Guid.Empty && model.RoomId != Guid.Empty)
                {
                    // Validate that room belongs to department
                    var selectedRoom = model.Rooms.FirstOrDefault(r => r.Id == model.RoomId && r.DepartmentId == model.DepartmentId);
                    if (selectedRoom == null)
                    {
                        TempData["ErrorMessage"] = "Phòng được chọn không thuộc khoa đã chọn.";
                        // Reset room selection
                        model.RoomId = Guid.Empty;
                        CreateEmptyWeekSchedule(model);
                        return View(model);
                    }

                    await LoadWeeklySchedule(model);
                    TempData["SuccessMessage"] = "Đã tải lịch làm việc thành công!";
                }
                else
                {
                    CreateEmptyWeekSchedule(model);
                    if (model.DepartmentId == Guid.Empty)
                    {
                        TempData["InfoMessage"] = "Vui lòng chọn khoa để xem danh sách phòng.";
                    }
                    else if (model.RoomId == Guid.Empty)
                    {
                        TempData["InfoMessage"] = "Vui lòng chọn phòng để xem lịch làm việc.";
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Manage POST with model: {@Model}", model);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải lịch làm việc: " + ex.Message;
                
                // Ensure we have minimum required data
                model.AvailableYears = Enumerable.Range(DateTime.Now.Year, 2).ToList();
                model.AvailableWeeks = Enumerable.Range(1, 53).ToList();
                await LoadDepartmentsAndRooms(model);
                CreateEmptyWeekSchedule(model);
                
                return View(model);
            }
        }

        // Action để đăng ký single schedule
        [HttpPost]
        public async Task<IActionResult> RegisterSchedule(Guid roomId, DateTime date, string period, int year, int week, Guid departmentId)
        {
            try
            {
                // Get default time slots for the period
                var timeSlotsResult = await _timeSlotService.GetTimeSlotsAsync();
                if (!timeSlotsResult.Success || timeSlotsResult.Data == null)
                {
                    TempData["ErrorMessage"] = "Không thể tải danh sách khung giờ.";
                    return RedirectToAction("Manage", new { departmentId, roomId, year, week });
                }

                var defaultTimeSlots = timeSlotsResult.Data
                    .Where(ts => ts.Period.Equals(period, StringComparison.OrdinalIgnoreCase))
                    .Select(ts => ts.Id)
                    .ToList();

                if (!defaultTimeSlots.Any())
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khung giờ cho ca làm việc này.";
                    return RedirectToAction("Manage", new { departmentId, roomId, year, week });
                }

                var request = new List<ScheduleCreateRequest>
                {
                    new ScheduleCreateRequest
                    {
                        RoomId = roomId,
                        Date = date,
                        Period = period,
                        TimeSlotIds = defaultTimeSlots
                    }
                };

                var result = await _scheduleService.CreateDoctorScheduleAsync(request);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"Đã đăng ký ca {(period == "morning" ? "sáng" : "chiều")} ngày {date:dd/MM/yyyy} thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage ?? "Đăng ký lịch thất bại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering schedule");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng ký lịch làm việc.";
            }

            return RedirectToAction("Manage", new { departmentId, roomId, year, week });
        }

        // Action để hủy single schedule
        [HttpPost]
        public async Task<IActionResult> CancelSchedule(Guid roomId, DateTime date, string period, int year, int week, Guid departmentId)
        {
            try
            {
                var request = new ScheduleDeleteRequest
                {
                    RoomId = roomId,
                    Date = date,
                    Period = period
                };

                var result = await _scheduleService.DeleteDoctorScheduleAsync(request);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"Đã hủy ca {(period == "morning" ? "sáng" : "chiều")} ngày {date:dd/MM/yyyy} thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage ?? "Hủy lịch thất bại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling schedule - RoomId: {RoomId}, Date: {Date}, Period: {Period}", roomId, date, period);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy lịch làm việc.";
            }

            return RedirectToAction("Manage", new { departmentId, roomId, year, week });
        }

        // Bulk Register Actions
        [HttpPost]
        public async Task<IActionResult> BulkRegisterWeek(Guid roomId, int year, int week, Guid departmentId, 
            List<string>? selectedDates, List<string>? periods)
        {
            try
            {
                if (selectedDates?.Any() != true || periods?.Any() != true)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một ngày và một ca làm việc.";
                    return RedirectToAction("Manage", new { departmentId, roomId, year, week });
                }

                var scheduleRequests = new List<ScheduleCreateRequest>();
                
                // Get default time slots for each period
                var timeSlotsResult = await _timeSlotService.GetTimeSlotsAsync();
                if (!timeSlotsResult.Success || timeSlotsResult.Data == null)
                {
                    TempData["ErrorMessage"] = "Không thể tải danh sách khung giờ.";
                    return RedirectToAction("Manage", new { departmentId, roomId, year, week });
                }

                foreach (var dateStr in selectedDates)
                {
                    if (DateTime.TryParse(dateStr, out var date))
                    {
                        foreach (var period in periods)
                        {
                            var defaultTimeSlots = timeSlotsResult.Data
                                .Where(ts => ts.Period.Equals(period, StringComparison.OrdinalIgnoreCase))
                                .Select(ts => ts.Id)
                                .ToList();

                            if (defaultTimeSlots.Any())
                            {
                                scheduleRequests.Add(new ScheduleCreateRequest
                                {
                                    RoomId = roomId,
                                    Date = date,
                                    Period = period,
                                    TimeSlotIds = defaultTimeSlots
                                });
                            }
                        }
                    }
                }

                if (scheduleRequests.Any())
                {
                    var result = await _scheduleService.CreateDoctorScheduleAsync(scheduleRequests);
                    
                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = $"Đã đăng ký thành công {scheduleRequests.Count} ca làm việc!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = result.ErrorMessage ?? "Có lỗi xảy ra khi đăng ký lịch.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Không có ca làm việc nào được tạo.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk register week");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng ký lịch làm việc.";
            }

            return RedirectToAction("Manage", new { departmentId, roomId, year, week });
        }

        [HttpPost]
        public async Task<IActionResult> BulkCancelWeek(Guid roomId, int year, int week, Guid departmentId)
        {
            try
            {
                // Get all schedules for this week
                var scheduleResult = await _scheduleService.GetDoctorScheduleAsync(roomId, year, week);
                
                if (scheduleResult.Success && scheduleResult.Data?.Any() == true)
                {
                    var deleteRequests = scheduleResult.Data
                        .GroupBy(s => new { s.Date, s.Period })
                        .Select(g => new ScheduleDeleteRequest
                        {
                            RoomId = roomId,
                            Date = g.Key.Date,
                            Period = g.Key.Period
                        })
                        .ToList();

                    var successCount = 0;
                    foreach (var request in deleteRequests)
                    {
                        var result = await _scheduleService.DeleteDoctorScheduleAsync(request);
                        if (result.Success) successCount++;
                    }

                    TempData["SuccessMessage"] = $"Đã hủy thành công {successCount} ca làm việc!";
                }
                else
                {
                    TempData["InfoMessage"] = "Không có ca làm việc nào để hủy trong tuần này.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk cancel week");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy lịch làm việc.";
            }

            return RedirectToAction("Manage", new { departmentId, roomId, year, week });
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAllMorning(Guid roomId, int year, int week, Guid departmentId)
        {
            return await RegisterPeriodForWeek(roomId, year, week, departmentId, "morning", "tất cả ca sáng");
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAllAfternoon(Guid roomId, int year, int week, Guid departmentId)
        {
            return await RegisterPeriodForWeek(roomId, year, week, departmentId, "afternoon", "tất cả ca chiều");
        }

        [HttpPost]
        public async Task<IActionResult> RegisterWorkdays(Guid roomId, int year, int week, Guid departmentId)
        {
            try
            {
                // Calculate week dates (Monday to Friday only)
                var jan1 = new DateTime(year, 1, 1);
                var daysOffset = (int)DayOfWeek.Monday - (int)jan1.DayOfWeek;
                var firstMonday = jan1.AddDays(daysOffset);
                var startOfWeek = firstMonday.AddDays((week - 1) * 7);
                var workdays = Enumerable.Range(0, 5).Select(i => startOfWeek.AddDays(i)).ToList(); // Monday to Friday

                var timeSlotsResult = await _timeSlotService.GetTimeSlotsAsync();
                if (!timeSlotsResult.Success || timeSlotsResult.Data == null)
                {
                    TempData["ErrorMessage"] = "Không thể tải danh sách khung giờ.";
                    return RedirectToAction("Manage", new { departmentId, roomId, year, week });
                }

                var scheduleRequests = new List<ScheduleCreateRequest>();

                foreach (var date in workdays)
                {
                    // Register both morning and afternoon for each workday
                    foreach (var period in new[] { "morning", "afternoon" })
                    {
                        var defaultTimeSlots = timeSlotsResult.Data
                            .Where(ts => ts.Period.Equals(period, StringComparison.OrdinalIgnoreCase))
                            .Select(ts => ts.Id)
                            .ToList();

                        if (defaultTimeSlots.Any())
                        {
                            scheduleRequests.Add(new ScheduleCreateRequest
                            {
                                RoomId = roomId,
                                Date = date,
                                Period = period,
                                TimeSlotIds = defaultTimeSlots
                            });
                        }
                    }
                }

                if (scheduleRequests.Any())
                {
                    var result = await _scheduleService.CreateDoctorScheduleAsync(scheduleRequests);
                    
                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = $"Đã đăng ký thành công tất cả ca làm việc từ Thứ 2 đến Thứ 6!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = result.ErrorMessage ?? "Có lỗi xảy ra khi đăng ký lịch.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Không có ca làm việc nào được tạo.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering workdays");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng ký lịch làm việc.";
            }

            return RedirectToAction("Manage", new { departmentId, roomId, year, week });
        }

        private async Task<IActionResult> RegisterPeriodForWeek(Guid roomId, int year, int week, Guid departmentId, string period, string periodDisplayName)
        {
            try
            {
                // Calculate week dates
                var jan1 = new DateTime(year, 1, 1);
                var daysOffset = (int)DayOfWeek.Monday - (int)jan1.DayOfWeek;
                var firstMonday = jan1.AddDays(daysOffset);
                var startOfWeek = firstMonday.AddDays((week - 1) * 7);
                var weekDates = Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i)).ToList();

                var timeSlotsResult = await _timeSlotService.GetTimeSlotsAsync();
                if (!timeSlotsResult.Success || timeSlotsResult.Data == null)
                {
                    TempData["ErrorMessage"] = "Không thể tải danh sách khung giờ.";
                    return RedirectToAction("Manage", new { departmentId, roomId, year, week });
                }

                var defaultTimeSlots = timeSlotsResult.Data
                    .Where(ts => ts.Period.Equals(period, StringComparison.OrdinalIgnoreCase))
                    .Select(ts => ts.Id)
                    .ToList();

                if (!defaultTimeSlots.Any())
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy khung giờ cho {periodDisplayName}.";
                    return RedirectToAction("Manage", new { departmentId, roomId, year, week });
                }

                var scheduleRequests = weekDates.Select(date => new ScheduleCreateRequest
                {
                    RoomId = roomId,
                    Date = date,
                    Period = period,
                    TimeSlotIds = defaultTimeSlots
                }).ToList();

                var result = await _scheduleService.CreateDoctorScheduleAsync(scheduleRequests);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"Đã đăng ký thành công {periodDisplayName} trong tuần!";
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage ?? $"Có lỗi xảy ra khi đăng ký {periodDisplayName}.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering period for week");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi đăng ký {periodDisplayName}.";
            }

            return RedirectToAction("Manage", new { departmentId, roomId, year, week });
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
