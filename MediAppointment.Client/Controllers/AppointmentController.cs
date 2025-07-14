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
        private readonly ITimeSlotService _timeSlotService;

        public AppointmentController(IAppointmentService appointmentService, IDoctorService doctorService, IAuthService authService, ITimeSlotService timeSlotService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
            _authService = authService;
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


    }
}
