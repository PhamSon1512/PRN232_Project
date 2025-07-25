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
        [HttpGet]
        public async Task<IActionResult> Index(int? year, string? week)
        {
            var allBookings = await _doctorAppointmentService.GetBookingsByDoctorAsync();

            var availableYears = allBookings
                .Select(b => b.AppointmentDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            var selectedYear = year ?? DateTime.Now.Year;

            // Lấy danh sách tuần dạng: "Tuần 01 (01/01 - 07/01)"
            var bookingsInYear = allBookings.Where(b => b.AppointmentDate.Year == selectedYear).ToList();
            var weekGroups = bookingsInYear
                .GroupBy(b => System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    b.AppointmentDate,
                    System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday))
                .OrderBy(g => g.Key)
                .ToList();

            var availableWeeks = weekGroups
                .Select(g =>
                {
                    var weekNum = g.Key;
                    var firstDayOfWeek = g.Min(x => x.AppointmentDate).Date;
                    var lastDayOfWeek = g.Max(x => x.AppointmentDate).Date;
                    return $"Tuần {weekNum:D2} ({firstDayOfWeek:dd/MM} - {lastDayOfWeek:dd/MM})";
                })
                .ToList();

            var filteredBookings = allBookings;

            if (year.HasValue)
            {
                filteredBookings = filteredBookings.Where(b => b.AppointmentDate.Year == year.Value).ToList();
            }

            if (!string.IsNullOrEmpty(week))
            {
                var weekNum = int.Parse(week.Split(' ')[1]);
                filteredBookings = filteredBookings
                    .Where(b => System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        b.AppointmentDate,
                        System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday) == weekNum)
                    .ToList();
            }

            ViewBag.AvailableYears = availableYears;
            ViewBag.AvailableWeeks = availableWeeks;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedWeek = week;

            return View(filteredBookings);
        }


        private List<string> GetWeeksOfYear(int year)
        {
            var weeks = new List<string>();
            var firstDate = new DateTime(year, 1, 1);

            while (firstDate.DayOfWeek != DayOfWeek.Monday)
                firstDate = firstDate.AddDays(1);

            while (firstDate.Year == year)
            {
                var start = firstDate;
                var end = start.AddDays(6);
                weeks.Add($"{start:dd/MM} To {end:dd/MM}");
                firstDate = firstDate.AddDays(7);
            }

            return weeks;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id, Guid timeSlotID, Guid patientId, Guid departmentId, DateTime appointmentDate)
        {
            var update = new BookingDoctorStatusUpdate
            {
                Status = "Approved",
                Note = "Lịch hẹn đã được chấp nhận.",
                TimeSlotID = timeSlotID,
                PatientID = patientId,
                DepartmentId = departmentId,
                AppointmentDate = appointmentDate
            };

            var result = await _doctorAppointmentService.UpdateBookingStatusAsync(id, update);

            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

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

            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction("Index");
        }

    }
}
