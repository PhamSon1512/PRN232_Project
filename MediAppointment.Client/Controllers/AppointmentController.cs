using MediAppointment.Client.Models.Appointment;
using MediAppointment.Client.Services;
using MediAppointment.Client.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.Client.Controllers
{
    [RequirePatient]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorService _doctorService;
        private readonly IAuthService _authService;
        private readonly IDepartmentService _departmentService;
        private readonly ITimeSlotService _timeSlotService;

        public AppointmentController(IAppointmentService appointmentService, IDoctorService doctorService, IAuthService authService, IDepartmentService departmentService, ITimeSlotService timeSlotService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
            _authService = authService;
            _departmentService = departmentService;
            _timeSlotService = timeSlotService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _appointmentService.GetMyAppointmentsAsync();
            
            if (result.Success)
            {
                return View(result.Data ?? new List<AppointmentViewModel>());
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải danh sách cuộc hẹn";
            return View(new List<AppointmentViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _appointmentService.GetAppointmentDetailAsync(id);
            
            if (result.Success && result.Data != null)
            {
                return View(result.Data);
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải chi tiết cuộc hẹn";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateAppointmentViewModel
            {
                AppointmentDate = DateTime.Today.AddDays(1)
            };

            // Load available doctors
            var doctorsResult = await _doctorService.GetAllDoctorsAsync();
            if (doctorsResult.Success && doctorsResult.Data != null)
            {
                model.AvailableDoctors = doctorsResult.Data.Select(d => new DoctorOptionViewModel
                {
                    Id = d.Id,
                    Name = d.FullName,
                    Department = string.Join(", ", d.Departments)
                }).ToList();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload doctors if validation fails
                var doctorsResult = await _doctorService.GetAllDoctorsAsync();
                if (doctorsResult.Success && doctorsResult.Data != null)
                {
                    model.AvailableDoctors = doctorsResult.Data.Select(d => new DoctorOptionViewModel
                    {
                        Id = d.Id,
                        Name = d.FullName,
                        Department = string.Join(", ", d.Departments)
                    }).ToList();
                }
                return View(model);
            }

            var result = await _appointmentService.CreateAppointmentAsync(model);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Data ?? "Đặt lịch hẹn thành công!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Đặt lịch hẹn thất bại");
            
            // Reload doctors for the view
            var doctorsReloadResult = await _doctorService.GetAllDoctorsAsync();
            if (doctorsReloadResult.Success && doctorsReloadResult.Data != null)
            {
                model.AvailableDoctors = doctorsReloadResult.Data.Select(d => new DoctorOptionViewModel
                {
                    Id = d.Id,
                    Name = d.FullName,
                    Department = string.Join(", ", d.Departments)
                }).ToList();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var result = await _appointmentService.CancelAppointmentAsync(id);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Data ?? "Hủy lịch hẹn thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Hủy lịch hẹn thất bại";
            }

            return RedirectToAction("Index");
        }        [HttpGet]
        public async Task<IActionResult> GetTimeSlots(Guid doctorId, DateTime date)
        {
            try
            {
                // Call API to get real time slots for doctor and date
                var result = await _timeSlotService.CheckTimeSlotAvailabilityAsync(doctorId, date);
                
                if (result.Success && result.Data != null)
                {
                    var timeSlots = result.Data.Select(slot => new TimeSlotOptionViewModel
                    {
                        RoomTimeSlotId = slot.Id,
                        TimeRange = slot.TimeRange,
                        RoomName = slot.RoomName ?? "Phòng khám",
                        IsAvailable = slot.IsAvailable
                    }).ToList();
                    
                    return Json(timeSlots);
                }
                
                // Return empty list if API fails or no data
                return Json(new List<TimeSlotOptionViewModel>());
            }
            catch (Exception)
            {
                // Log error and return empty list
                return Json(new List<TimeSlotOptionViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Book(Guid? departmentId, int? year, int? week)
        {
            var model = new PatientBookingViewModel
            {
                DepartmentId = departmentId ?? Guid.Empty,
                Year = year ?? DateTime.Now.Year,
                Week = week ?? GetWeekOfYear(DateTime.Now),
                AvailableYears = Enumerable.Range(DateTime.Now.Year, 2).ToList(),
                AvailableWeeks = Enumerable.Range(1, 53).ToList()
            };

            // Load departments
            await LoadDepartments(model);

            // Load time slots if filters are applied
            if (departmentId.HasValue && year.HasValue && week.HasValue)
            {
                await LoadWeeklyTimeSlots(model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(PatientBookingViewModel request)
        {
            try
            {
                var createRequest = new CreateAppointmentViewModel
                {
                    RoomTimeSlotId = request.TimeSlotId,
                    AppointmentDate = request.Date,
                    Note = "Đặt lịch từ trang đặt lịch bệnh nhân",
                    DoctorId = Guid.Empty // This will need to be derived from the time slot or department
                };
                
                var result = await _appointmentService.CreateAppointmentAsync(createRequest);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Đặt lịch hẹn thành công!" });
                }

                return Json(new { success = false, message = result.ErrorMessage ?? "Đặt lịch hẹn thất bại" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        private async Task LoadDepartments(PatientBookingViewModel model)
        {
            try
            {
                var result = await _departmentService.GetDepartmentsAsync();
                if (result.Success && result.Data != null)
                {
                    model.Departments = result.Data;
                }
                else
                {
                    // Return empty list if API fails
                    model.Departments = new List<DepartmentOption>();
                }
            }
            catch (Exception)
            {
                model.Departments = new List<DepartmentOption>();
            }
        }

        private async Task LoadWeeklyTimeSlots(PatientBookingViewModel model)
        {
            try
            {
                // Calculate week dates
                var jan1 = new DateTime(model.Year, 1, 1);
                var daysOffset = (int)DayOfWeek.Monday - (int)jan1.DayOfWeek;
                var firstMonday = jan1.AddDays(daysOffset);
                var startOfWeek = firstMonday.AddDays((model.Week - 1) * 7);
                var endOfWeek = startOfWeek.AddDays(6);

                // Call API to get available time slots for the week
                var result = await _timeSlotService.GetAvailableTimeSlotsAsync(model.DepartmentId, startOfWeek, endOfWeek);
                
                if (result.Success && result.Data != null)
                {
                    // Group time slots by date
                    model.WeeklyTimeSlots = result.Data
                        .GroupBy(slot => slot.Date.Date)
                        .ToDictionary(g => g.Key, g => g.ToList());
                }
                else
                {
                    // Return empty dictionary if API fails
                    model.WeeklyTimeSlots = new Dictionary<DateTime, List<TimeSlotOption>>();
                }
            }
            catch (Exception)
            {
                model.WeeklyTimeSlots = new Dictionary<DateTime, List<TimeSlotOption>>();
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
