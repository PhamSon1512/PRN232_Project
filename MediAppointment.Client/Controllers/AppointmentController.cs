using MediAppointment.Client.Models.Appointment;
using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.Client.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorService _doctorService;
        private readonly IAuthService _authService;

        public AppointmentController(IAppointmentService appointmentService, IDoctorService doctorService, IAuthService authService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            if (!isAuthenticated)
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path });
            }

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
            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            if (!isAuthenticated)
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path });
            }

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
            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            if (!isAuthenticated)
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path });
            }

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
            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            if (!isAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

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
            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            if (!isAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

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
        public IActionResult GetTimeSlots(Guid doctorId, DateTime date)
        {
            // This would typically call an API to get available time slots for a doctor on a specific date
            // For now, return mock data
            var timeSlots = new List<TimeSlotOptionViewModel>
            {
                new() { RoomTimeSlotId = Guid.NewGuid(), TimeRange = "08:00 - 09:00", RoomName = "Phòng 101", IsAvailable = true },
                new() { RoomTimeSlotId = Guid.NewGuid(), TimeRange = "09:00 - 10:00", RoomName = "Phòng 101", IsAvailable = true },
                new() { RoomTimeSlotId = Guid.NewGuid(), TimeRange = "10:00 - 11:00", RoomName = "Phòng 101", IsAvailable = false },
                new() { RoomTimeSlotId = Guid.NewGuid(), TimeRange = "14:00 - 15:00", RoomName = "Phòng 102", IsAvailable = true },
                new() { RoomTimeSlotId = Guid.NewGuid(), TimeRange = "15:00 - 16:00", RoomName = "Phòng 102", IsAvailable = true }
            };

            return Json(timeSlots);
        }
    }
}
