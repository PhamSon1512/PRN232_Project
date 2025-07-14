using MediAppointment.Client.Models.Appointment;
using MediAppointment.Client.Services;
using MediAppointment.Client.Attributes;
using Microsoft.AspNetCore.Mvc;
using TimeSlotOption = MediAppointment.Client.Models.Appointment.TimeSlotOption;

namespace MediAppointment.Client.Controllers
{
    [RequirePatient] // Chỉ patient mới được book
    public class BookingController : Controller
    {
        private readonly IAppointmentService _appointmentService;

        public BookingController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new BookingViewModel();
            
            // Load departments
            var departmentsResult = await _appointmentService.GetDepartmentsAsync();
            if (departmentsResult.Success && departmentsResult.Data != null)
            {
                model.Departments = departmentsResult.Data;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoadTimeSlots([FromBody] TimeSlotRequest request)
        {
            try
            {
                // Chỉ load cho ngày được chọn, không cần load cả tuần
                var result = await _appointmentService.GetAvailableTimeSlotsAsync(
                    request.DepartmentId, 
                    request.StartDate, 
                    request.StartDate); // StartDate = EndDate để chỉ load 1 ngày

                if (result.Success && result.Data != null)
                {
                    // Lọc chỉ những time slots có sẵn và group theo ngày thực tế
                    var groupedByDate = result.Data
                        .Where(ts => ts.Date.Date == request.StartDate.Date) // Chỉ lấy ngày được chọn
                        .GroupBy(ts => ts.Date.Date) // Group theo ngày thực tế từ response
                        .ToDictionary(g => g.Key, g => g.ToList());

                    return Json(new { success = true, timeSlots = groupedByDate });
                }

                return Json(new { success = false, message = result.ErrorMessage ?? "Không thể tải lịch khám" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload departments
                var departmentsResult = await _appointmentService.GetDepartmentsAsync();
                if (departmentsResult.Success && departmentsResult.Data != null)
                {
                    model.Departments = departmentsResult.Data;
                }
                return View("Index", model);
            }

            var result = await _appointmentService.BookAppointmentAsync(model);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Data ?? "Đặt lịch khám thành công!";
                return RedirectToAction("Index", "Appointment");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Đặt lịch thất bại");
            
            // Reload departments
            var departmentsReloadResult = await _appointmentService.GetDepartmentsAsync();
            if (departmentsReloadResult.Success && departmentsReloadResult.Data != null)
            {
                model.Departments = departmentsReloadResult.Data;
            }

            return View("Index", model);
        }
    }

    // Supporting request models
    public class TimeSlotRequest
    {
        public Guid DepartmentId { get; set; }
        public DateTime StartDate { get; set; }
    }
}
