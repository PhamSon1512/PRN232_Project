using MediAppointment.Client.Attributes;
using MediAppointment.Client.Models.Appointment;
using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.Client.Controllers
{
    [RequireDoctor]
    public class DoctorMyBookingHomeController : Controller
    {
        private readonly IDoctorAppointmentService _doctorAppointmentService;

        public DoctorMyBookingHomeController(IDoctorAppointmentService doctorAppointmentService)
        {
            _doctorAppointmentService = doctorAppointmentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var bookings = await _doctorAppointmentService.GetBookingsByDoctorAsync();

            if (bookings != null && bookings.Any())
            {
                return View(bookings);
            }

            TempData["InfoMessage"] = "Hiện tại bạn chưa có lịch hẹn nào.";
            return View(new List<BookingDoctorView>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            var update = new BookingDoctorStatusUpdate
            {
                Status = "Approved",
                Note = "Lịch hẹn đã được chấp nhận." // Có thể để null nếu không cần ghi chú
            };

            var result = await _doctorAppointmentService.UpdateBookingStatusAsync(id, update);

            TempData[result ? "SuccessMessage" : "ErrorMessage"] = result
                ? "Đã chấp nhận lịch hẹn."
                : "Chấp nhận lịch hẹn thất bại.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id, string note)
        {
            var update = new BookingDoctorStatusUpdate
            {
                Status = "Rejected",
                Note = note
            };

            var result = await _doctorAppointmentService.UpdateBookingStatusAsync(id, update);

            TempData[result ? "SuccessMessage" : "ErrorMessage"] = result
                ? "Đã từ chối lịch hẹn."
                : "Từ chối lịch hẹn thất bại.";

            return RedirectToAction("Index");
        }


    }
}
