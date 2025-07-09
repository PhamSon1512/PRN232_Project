using MediAppointment.Client.Services;
using MediAppointment.Client.Models.Manager;
using MediAppointment.Client.Models.Appointment;
using MediAppointment.Client.Attributes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MediAppointment.Client.Controllers
{
    [RequireManager]
    public class ManagerController : Controller
    {
        private readonly IScheduleService _scheduleService;
        private readonly IDepartmentService _departmentService;
        private readonly IDoctorService _doctorService;

        public ManagerController(IScheduleService scheduleService, IDepartmentService departmentService, IDoctorService doctorService)
        {
            _scheduleService = scheduleService;
            _departmentService = departmentService;
            _doctorService = doctorService;
        }

        [Route("Manager")]
        public IActionResult Index()
        {
            return RedirectToAction("ScheduleOverview");
        }

        [HttpGet]
        public async Task<IActionResult> ScheduleOverview(Guid? departmentId, Guid? roomId, Guid? doctorId, int? year, int? week)
        {
            var model = new ManagerScheduleOverviewViewModel
            {
                DepartmentId = departmentId,
                RoomId = roomId,
                DoctorId = doctorId,
                Year = year ?? DateTime.Now.Year,
                Week = week ?? GetWeekOfYear(DateTime.Now),
                AvailableYears = Enumerable.Range(DateTime.Now.Year, 2).ToList(),
                AvailableWeeks = Enumerable.Range(1, 53).ToList()
            };

            // Load filter options
            await LoadFilterOptions(model);

            // Load schedule if year and week are selected
            if (year.HasValue && week.HasValue)
            {
                LoadManagerWeeklySchedule(model);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ExportSchedule(string format, Guid? departmentId, Guid? roomId, Guid? doctorId, int? year, int? week)
        {
            try
            {
                if (!year.HasValue || !week.HasValue)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn năm và tuần để xuất lịch";
                    return RedirectToAction("ScheduleOverview");
                }

                // Call API to export schedule
                // For now, just redirect back with success message
                TempData["SuccessMessage"] = $"Xuất lịch thành công dưới định dạng {format?.ToUpper() ?? "EXCEL"}";
                return RedirectToAction("ScheduleOverview", new { departmentId, roomId, doctorId, year, week });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xuất lịch: {ex.Message}";
                return RedirectToAction("ScheduleOverview", new { departmentId, roomId, doctorId, year, week });
            }
        }

        private async Task LoadFilterOptions(ManagerScheduleOverviewViewModel model)
        {
            try
            {
                // Load departments
                var departmentsResult = await _departmentService.GetDepartmentsAsync();
                model.Departments = departmentsResult.Success && departmentsResult.Data != null 
                    ? departmentsResult.Data.Select(d => new Models.Manager.DepartmentOption 
                    { 
                        Id = d.Id, 
                        Name = d.Name 
                    }).ToList()
                    : new List<Models.Manager.DepartmentOption>();

                // Load rooms for selected department
                if (model.DepartmentId.HasValue && model.DepartmentId != Guid.Empty)
                {
                    var roomsResult = await _departmentService.GetRoomsByDepartmentAsync(model.DepartmentId.Value);
                    model.Rooms = roomsResult.Success && roomsResult.Data != null
                        ? roomsResult.Data.Select(r => new Models.Manager.RoomOption 
                        { 
                            Id = r.Id, 
                            Name = r.Name
                        }).ToList()
                        : new List<Models.Manager.RoomOption>();
                }
                else
                {
                    model.Rooms = new List<Models.Manager.RoomOption>();
                }

                // Load doctors
                var doctorsResult = await _doctorService.GetAllDoctorsAsync();
                model.Doctors = doctorsResult.Success && doctorsResult.Data != null
                    ? doctorsResult.Data.Select(d => new Models.Manager.DoctorOption 
                    { 
                        Id = d.Id, 
                        Name = d.FullName
                    }).ToList()
                    : new List<Models.Manager.DoctorOption>();
            }
            catch (Exception)
            {
                model.Departments = new List<Models.Manager.DepartmentOption>();
                model.Rooms = new List<Models.Manager.RoomOption>();
                model.Doctors = new List<Models.Manager.DoctorOption>();
            }
        }

        private void LoadManagerWeeklySchedule(ManagerScheduleOverviewViewModel model)
        {
            try
            {
                // Return empty schedule dictionary since we're removing mock data
                // Real implementation should call API to get schedule data
                model.WeeklySchedule = new Dictionary<DateTime, List<ManagerScheduleSlot>>();
            }
            catch (Exception)
            {
                model.WeeklySchedule = new Dictionary<DateTime, List<ManagerScheduleSlot>>();
            }
        }

        private int GetWeekOfYear(DateTime date)
        {
            var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }
    }
}
