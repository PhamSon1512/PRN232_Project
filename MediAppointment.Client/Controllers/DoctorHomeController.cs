using MediAppointment.Client.Models.Doctor;
using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.Client.Controllers
{
    public class DoctorHomeController : Controller
    {
        private readonly IDoctorAppointmentService _doctorAppointmentService;

        public DoctorHomeController(IDoctorAppointmentService doctorAppointmentService)
        {
            _doctorAppointmentService = doctorAppointmentService;
        }

        [Route("DoctorHome")]
        public async Task<IActionResult> Index(int? year, string? week)
        {
            // Mặc định năm là 2025
            var currentYear = year ?? 2025;

            // Lấy toàn bộ tuần của năm
            var allWeeks = GetWeeksOfYear(currentYear);

            // Nếu chưa chọn tuần → chọn tuần hiện tại theo ngày hôm nay
            var today = DateTime.Today;
            var selectedWeek = week;

            if (string.IsNullOrEmpty(week))
            {
                var firstMonday = allWeeks.Select(w =>
                {
                    var parts = w.Split(" To ");
                    return DateTime.ParseExact(parts[0] + "/" + currentYear, "dd/MM/yyyy", null);
                }).FirstOrDefault(start => today >= start && today <= start.AddDays(6));

                selectedWeek = allWeeks.FirstOrDefault(w =>
                {
                    var parts = w.Split(" To ");
                    var start = DateTime.ParseExact(parts[0] + "/" + currentYear, "dd/MM/yyyy", null);
                    var end = DateTime.ParseExact(parts[1] + "/" + currentYear, "dd/MM/yyyy", null);
                    return today >= start && today <= end;
                }) ?? allWeeks.Last(); // fallback nếu không tìm được
            }

            // Tính khoảng ngày từ chuỗi tuần
            var dateParts = selectedWeek.Split(" To ");
            var startDate = DateTime.ParseExact(dateParts[0] + "/" + currentYear, "dd/MM/yyyy", null);
            var endDate = DateTime.ParseExact(dateParts[1] + "/" + currentYear, "dd/MM/yyyy", null);

            var assignedSlots = (await _doctorAppointmentService.GetAssignedSlotsAsync())
                .Where(x => x.Date.Date >= startDate && x.Date.Date <= endDate)
                .ToList();

            // Phân chia sáng/chiều
            var morningSlots = assignedSlots
                .Where(x => x.Shift == "Morning")
                .GroupBy(x => x.Date.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new ScheduleSlot
                    {
                        RoomName = x.RoomName,
                        TimeStart = x.TimeStart.ToString(@"hh\:mm"),
                        TimeEnd = x.TimeEnd.ToString(@"hh\:mm"),
                        Date = x.Date
                    }).ToList());

            var afternoonSlots = assignedSlots
                .Where(x => x.Shift == "Afternoon")
                .GroupBy(x => x.Date.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new ScheduleSlot
                    {
                        RoomName = x.RoomName,
                        TimeStart = x.TimeStart.ToString(@"hh\:mm"),
                        TimeEnd = x.TimeEnd.ToString(@"hh\:mm"),
                        Date = x.Date
                    }).ToList());

            var viewModel = new Doctor_AssginedScheduleViewModel
            {
                SelectedYear = currentYear,
                SelectedWeek = selectedWeek,
                AvailableYears = Enumerable.Range(2023, 5).ToList(),
                AvailableWeeks = allWeeks,
                MorningSlots = morningSlots,
                AfternoonSlots = afternoonSlots
            };

            ViewBag.StartDate = startDate;
            return View(viewModel);
        }



        private List<string> GetWeeksOfYear(int year)
        {
            var weeks = new List<string>();
            var firstDate = new DateTime(year, 1, 1);

            // Bắt đầu từ thứ 2 đầu tiên
            while (firstDate.DayOfWeek != DayOfWeek.Monday)
                firstDate = firstDate.AddDays(1);

            while (firstDate.Year == year)
            {
                var start = firstDate;
                var end = firstDate.AddDays(6);
                weeks.Add($"{start:dd/MM} To {end:dd/MM}");
                firstDate = firstDate.AddDays(7);
            }

            return weeks;
        }

    }
}
