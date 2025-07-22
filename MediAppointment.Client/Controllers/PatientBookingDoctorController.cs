using MediAppointment.Client.Attributes;
using MediAppointment.Client.Models.Appointment;
using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.Client.Controllers
{
    [RequirePatient]
    public class PatientBookingDoctorController : Controller
    {
        private readonly IBookingDoctorService _bookingDoctorService;

        public PatientBookingDoctorController(IBookingDoctorService bookingDoctorService)
        {
            _bookingDoctorService = bookingDoctorService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var bookings = await _bookingDoctorService.GetBookingsByPatientAsync();

            if (bookings != null && bookings.Any())
            {
                return View(bookings);
            }

            TempData["InfoMessage"] = "Bạn chưa có lịch hẹn nào với bác sĩ.";
            return View(new List<BookingDoctorView>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var result = await _bookingDoctorService.CancelBookingAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "Hủy lịch hẹn thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy lịch hẹn. Vui lòng thử lại.";
            }

            return RedirectToAction("Index");
        }
    }
}
