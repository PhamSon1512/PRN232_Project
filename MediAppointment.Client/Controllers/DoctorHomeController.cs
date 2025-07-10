using MediAppointment.Client.Models.Appointment;
using MediAppointment.Client.Models.Doctor;
using MediAppointment.Client.Models.MedicalRecord;
using MediAppointment.Client.Services;
using MediAppointment.Client.Attributes;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MediAppointment.Client.Controllers
{
    [RequireDoctor]
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


        [Route("DoctorHome/Details")]
        public async Task<IActionResult> Details(string date, string period)
        {
            if (!DateTime.TryParse(date, out var selectedDate))
            {
                return BadRequest("Ngày không hợp lệ.");
            }

            // Lấy "week" từ query string
            string selectedWeek = Request.Query["week"];
            int selectedYear = selectedDate.Year;

            var allSlots = await _doctorAppointmentService.GetAssignedSlotsAsync();

            var filteredSlots = allSlots
                .Where(s => s.Date.Date == selectedDate.Date && s.Shift.Equals(period, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.TimeStart)
                .ToList();

            var viewModel = new Doctor_SlotListViewModel
            {
                Date = selectedDate,
                Shift = period,
                Slots = filteredSlots,
                SelectedYear = selectedYear,
                SelectedWeek = selectedWeek
            };

            return View(viewModel);
        }


        [Route("DoctorHome/PatientDetail")]
        public async Task<IActionResult> PatientDetail(Guid roomTimeSlotId, string week, int year, DateTime date, string period)
        {
            // Lấy thông tin các cuộc hẹn theo slot
            var slotAppointments = await _doctorAppointmentService.GetAppointmentsBySlotAsync(roomTimeSlotId);

            // Khởi tạo các biến mặc định
            Doctor_AppointmentDetailViewModel? firstAppointment = null;
            Doctor_PatientDetailViewModel? patientDetail = null;

            // Kiểm tra và lấy cuộc hẹn đầu tiên nếu có
            if (slotAppointments?.Appointments != null && slotAppointments.Appointments.Any())
            {
                firstAppointment = slotAppointments.Appointments.First();

                // Lấy thông tin bệnh nhân nếu có cuộc hẹn
                if (firstAppointment != null)
                {
                    patientDetail = await _doctorAppointmentService.GetPatientDetailAsync(firstAppointment.PatientId);

                    if (patientDetail == null)
                    {
                        ViewBag.Message = "Không tìm thấy thông tin bệnh nhân.";
                    }
                }
            }
            else
            {
                ViewBag.Message = "Hiện tại chưa có bệnh nhân nào đặt lịch cho ca làm này.";
            }

            // Chuẩn bị ViewModel
            var viewModel = new Doctor_PatientInfoViewModel
            {
                AppointmentSlot = slotAppointments,
                AppointmentDetail = firstAppointment,
                Patient = patientDetail,
                Week = week,
                Year = year,
                Date = date,
                Period = period
            };

            return View(viewModel);
        }



        [HttpGet]
        public IActionResult CreateMedicalRecord(Guid patientId, Guid roomTimeSlotId, string week, int year, DateTime date, string period)
        {
            var model = new CreateMedicalRecordRequest
            {
                PatientId = patientId
            };

            ViewBag.RoomTimeSlotId = roomTimeSlotId;
            ViewBag.Week = week;
            ViewBag.Year = year;
            ViewBag.Date = date;
            ViewBag.Period = period;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedicalRecord(
           CreateMedicalRecordRequest request,
           Guid roomTimeSlotId,
           string week,
           int year,
           DateTime date,
           string period)
        {
            if (!ModelState.IsValid)
            {
                // Gửi lại dữ liệu vào ViewBag để render lại View nếu ModelState không hợp lệ
                ViewBag.RoomTimeSlotId = roomTimeSlotId;
                ViewBag.Week = week;
                ViewBag.Year = year;
                ViewBag.Date = date;
                ViewBag.Period = period;

                return View(request);
            }

            try
            {
                var result = await _doctorAppointmentService.CreateMedicalRecordAsync(request);

                if (result)
                {
                    // Thông báo thành công
                    TempData["Success"] = "Thêm hồ sơ bệnh án thành công!";

                    return RedirectToAction("PatientDetail", new
                    {
                        patientId = request.PatientId,
                        roomTimeSlotId = roomTimeSlotId,
                        week = week,
                        year = year,
                        date = date.ToString("yyyy-MM-dd"),
                        period = period
                    });
                }

                // Nếu result = false
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi thêm hồ sơ.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi hệ thống: " + ex.Message);
            }

            // Nếu có lỗi, render lại view với dữ liệu
            ViewBag.RoomTimeSlotId = roomTimeSlotId;
            ViewBag.Week = week;
            ViewBag.Year = year;
            ViewBag.Date = date;
            ViewBag.Period = period;

            return View(request);
        }

    }
}