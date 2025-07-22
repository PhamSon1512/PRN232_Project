using MediAppointment.Client.Services;
using MediAppointment.Client.Models.Manager;
using MediAppointment.Client.Models.Appointment;
using MediAppointment.Client.Attributes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Claims;
using MediAppointment.Client.Models.Doctor;
using System.IdentityModel.Tokens.Jwt;
using ClosedXML.Excel;

namespace MediAppointment.Client.Controllers
{
    [RequireManager]
    public class ManagerController : Controller
    {
        private readonly IScheduleService _scheduleService;
        private readonly IDepartmentService _departmentService;
        private readonly IDoctorService _doctorService;
        private readonly IManagerService _managerService;

        public ManagerController(IScheduleService scheduleService, IDepartmentService departmentService, IDoctorService doctorService, IManagerService managerService)
        {
            _scheduleService = scheduleService;
            _departmentService = departmentService;
            _doctorService = doctorService;
            _managerService = managerService;
        }

        [Route("Manager")]
        public IActionResult Index()
        {
            return RedirectToAction("ScheduleOverview");
        }

        #region Management Doctor
        [HttpGet("Manager/DoctorManagement")]
        public async Task<IActionResult> DoctorManagement(string text = "", /*string department = "",*/ int page = 1, int pageSize = 5)
        {
            var result = await _managerService.GetAllDoctorsAsync(text, /*department,*/ page, pageSize);
            if (result.Success && result.Data != null)
            {
                ViewBag.Text = text;
                /*ViewBag.Department = department*/
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                return View("~/Views/Manager/DoctorManagement.cshtml", result.Data);
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải danh sách bác sĩ";
            return View("~/Views/Manager/DoctorManagement.cshtml");
        }

        [HttpGet("Manager/Doctors/{doctorId:guid}")]
        public async Task<IActionResult> DoctorDetails(Guid doctorId)
        {
            var result = await _managerService.GetDoctorByIdAsync(doctorId);
            if (result.Success && result.Data != null)
            {
                return View("~/Views/Manager/DoctorDetails.cshtml", result.Data);
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải thông tin bác sĩ";
            return RedirectToAction("DoctorManagement");
        }

        [HttpGet("Manager/Doctors/{doctorId:guid}/Edit")]
        public async Task<IActionResult> EditDoctor(Guid doctorId)
        {
            var result = await _managerService.GetDoctorByIdAsync(doctorId);
            var deptResult = await _departmentService.GetDepartmentsAsync();
            if (result.Success && result.Data != null)
            {
                var model = new DoctorUpdateModel
                {
                    FullName = result.Data.FullName,
                    PhoneNumber = result.Data.PhoneNumber,
                    Status = (result.Data as DoctorStatusModel)?.Status ?? 1,
                    Departments = result.Data.Departments
                        .Select(d => deptResult.Data.FirstOrDefault(dep => dep.Name == d)?.Id ?? Guid.Empty)
                        .Where(id => id != Guid.Empty)
                        .ToList()
                };
                ViewBag.Departments = deptResult.Success && deptResult.Data != null
                    ? deptResult.Data
                    : new List<MediAppointment.Client.Models.Appointment.DepartmentOption>();
                ViewBag.DoctorId = doctorId;
                return View("~/Views/Manager/EditDoctor.cshtml", model);
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải thông tin bác sĩ";
            return RedirectToAction("DoctorManagement");
        }

        [HttpPost("Manager/Doctors/{doctorId:guid}/Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDoctor(Guid doctorId, DoctorUpdateModel dto)
        {
            var managerId = GetManagerIdFromSession();
            if (managerId == null || managerId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Không thể xác định ID quản lý từ token. Vui lòng đăng nhập lại.";
                Console.WriteLine("Invalid manager ID in token");
                return RedirectToAction("DoctorManagement");
            }

            if (!ModelState.IsValid)
            {
                var deptResult = await _departmentService.GetDepartmentsAsync();
                ViewBag.Departments = deptResult.Success ? deptResult.Data : new List<MediAppointment.Client.Models.Appointment.DepartmentOption>();
                ViewBag.DoctorId = doctorId;
                return View("~/Views/Manager/EditDoctor.cshtml", dto);
            }

            try
            {
                var resultUpdate = await _managerService.UpdateDoctorProfileAsync(doctorId, dto);
                if (resultUpdate.Success && resultUpdate.Data != null)
                {
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ bác sĩ thành công!";
                    return RedirectToAction("DoctorDetails", new { doctorId });
                }

                TempData["ErrorMessage"] = resultUpdate.ErrorMessage ?? "Cập nhật hồ sơ bác sĩ thất bại";
                var deptResult = await _departmentService.GetDepartmentsAsync();
                ViewBag.Departments = deptResult.Success ? deptResult.Data : new List<MediAppointment.Client.Models.Appointment.DepartmentOption>();
                ViewBag.DoctorId = doctorId;
                return View("~/Views/Manager/EditDoctor.cshtml", dto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                var deptResult = await _departmentService.GetDepartmentsAsync();
                ViewBag.Departments = deptResult.Success ? deptResult.Data : new List<MediAppointment.Client.Models.Appointment.DepartmentOption>();
                ViewBag.DoctorId = doctorId;
                return View("~/Views/Manager/EditDoctor.cshtml", dto);
            }
        }

        [HttpGet("Manager/Doctors/Create")]
        public async Task<IActionResult> CreateDoctor()
        {
            var result = await _departmentService.GetDepartmentsAsync();
            ViewBag.Departments = result.Success ? result.Data : new List<MediAppointment.Client.Models.Appointment.DepartmentOption>();
            return View("~/Views/Manager/CreateDoctor.cshtml", new DoctorCreateModel());
        }

        [HttpPost("Manager/Doctors/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(DoctorCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                var result = await _departmentService.GetDepartmentsAsync();
                ViewBag.Departments = result.Success ? result.Data : new List<MediAppointment.Client.Models.Appointment.DepartmentOption>();
                return View("~/Views/Manager/CreateDoctor.cshtml", model);
            }

            var managerId = GetManagerIdFromSession();
            if (managerId == null || managerId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Không thể xác định ID quản lý từ token. Vui lòng đăng nhập lại.";
                Console.WriteLine("Invalid manager ID in token");
                return RedirectToAction("DoctorManagement");
            }

            try
            {
                var dto = new DoctorCreateModel
                {
                    FullName = model.FullName,
                    Gender = model.Gender,
                    DateOfBirth = model.DateOfBirth,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Departments = model.Departments ?? new List<Guid>()
                };

                var doctorId = await _managerService.CreateDoctorAsync(dto);
                TempData["SuccessMessage"] = "Tạo bác sĩ thành công!";
                return RedirectToAction("DoctorManagement");
            }
            catch (Exception ex)
            {
                if (ex.Message == "Email is already taken.")
                {
                    TempData["ErrorMessage"] = "Email đã tồn tại.";
                    return RedirectToAction("DoctorManagement");
                }
                else
                {
                    TempData["ErrorMessage"] = ex.Message;
                    var result = await _departmentService.GetDepartmentsAsync();
                    ViewBag.Departments = result.Success ? result.Data : new List<MediAppointment.Client.Models.Appointment.DepartmentOption>();
                    return View("~/Views/Manager/CreateDoctor.cshtml", model);
                }
            }
        }

        [HttpPost("Manager/Doctors/{doctorId:guid}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctor(Guid doctorId)
        {
            var managerId = GetManagerIdFromSession();
            if (managerId == null || managerId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Không thể xác định ID quản lý từ token. Vui lòng đăng nhập lại.";
                Console.WriteLine("Invalid manager ID in token");
                return RedirectToAction("DoctorManagement");
            }

            var result = await _managerService.DeleteDoctorAsync(doctorId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Xóa bác sĩ thành công!";
                return RedirectToAction("DoctorManagement");
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Xóa bác sĩ thất bại";
            return RedirectToAction("DoctorDetails", new { doctorId });
        }
        #endregion

        #region Manager Profile
        [HttpGet]
        public async Task<IActionResult> ManagerProfile()
        {
            var result = await _managerService.GetManagerProfileAsync();

            if (result.Success && result.Data != null)
            {
                return View("~/Views/Account/ManagerProfile.cshtml", result.Data);
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải thông tin hồ sơ Manager";
            return RedirectToAction("DoctorManagement", "Manager");
        }

        [HttpGet("Manager/EditProfile")]
        public async Task<IActionResult> EditProfile()
        {
            var result = await _managerService.GetManagerProfileAsync();
            if (result.Success && result.Data != null)
            {
                var model = new ManagerViewModel
                {
                    Id = result.Data.Id,
                    FullName = result.Data.FullName,
                    Email = result.Data.Email,
                    PhoneNumber = result.Data.PhoneNumber,
                    Role = result.Data.Role
                };
                return View("~/Views/Manager/EditProfile.cshtml", model);
            }
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải thông tin hồ sơ Manager";
            return RedirectToAction("ManagerProfile");
        }

        [HttpPost("Manager/UpdateProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ManagerUpdateProfile dto)
        {
            // Lấy ManagerId từ session
            var managerId = GetManagerIdFromSession();
            if (managerId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Không thể xác định ID quản lý từ session. Vui lòng đăng nhập lại.";
                return RedirectToAction("ManagerProfile");
            }
            dto.ManagerId = managerId;

            // Lấy profile hiện tại để điền giá trị nếu DTO rỗng
            var currentProfile = await _managerService.GetManagerProfileAsync();
            if (currentProfile.Success && currentProfile.Data != null)
            {
                if (string.IsNullOrWhiteSpace(dto.FullName))
                    dto.FullName = currentProfile.Data.FullName;
                if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    dto.PhoneNumber = currentProfile.Data.PhoneNumber;
            }

            if (!ModelState.IsValid)
            {
                if (currentProfile.Success && currentProfile.Data != null)
                {
                    var model = new ManagerViewModel
                    {
                        Id = currentProfile.Data.Id,
                        FullName = dto.FullName ?? currentProfile.Data.FullName,
                        Email = currentProfile.Data.Email,
                        PhoneNumber = dto.PhoneNumber ?? currentProfile.Data.PhoneNumber,
                        Role = currentProfile.Data.Role,
                        ErrorMessage = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .FirstOrDefault()
                    };
                    return View("EditProfile", model);
                }
                TempData["ErrorMessage"] = "Không thể tải thông tin hồ sơ của quản lý.";
                return RedirectToAction("ManagerProfile");
            }

            var resultUpdate = await _managerService.UpdateManagerProfileAsync(dto);
            if (resultUpdate.Success)
            {
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("ManagerProfile");
            }

            TempData["ErrorMessage"] = resultUpdate.ErrorMessage ?? "Cập nhật hồ sơ thất bại.";
            if (currentProfile.Success && currentProfile.Data != null)
            {
                var model = new ManagerViewModel
                {
                    Id = currentProfile.Data.Id,
                    FullName = dto.FullName ?? currentProfile.Data.FullName,
                    Email = currentProfile.Data.Email,
                    PhoneNumber = dto.PhoneNumber ?? currentProfile.Data.PhoneNumber,
                    Role = currentProfile.Data.Role
                };
                return View("EditProfile", model);
            }
            return RedirectToAction("ManagerProfile");
        }

        private Guid? GetManagerIdFromSession()
        {
            try
            {
                // Lấy token từ session
                var token = HttpContext.Session.GetString("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                // Decode JWT token
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                // Lấy UserId từ claims
                var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var adminId))
                {
                    return null;
                }

                return adminId;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region Schedule Management
        [HttpGet]
        public async Task<IActionResult> ScheduleOverview(Guid? departmentId, Guid? roomId, Guid? doctorId, int? year, int? week)
        {
            var now = DateTime.Now;
            var model = new ManagerScheduleOverviewViewModel
            {
                DepartmentId = departmentId,
                RoomId = roomId,
                DoctorId = doctorId,
                Year = year ?? now.Year, // Giá trị mặc định là năm hiện tại
                Week = week ?? GetWeekOfYear(now), // Giá trị mặc định là tuần hiện tại
                AvailableYears = Enumerable.Range(now.Year, 2).ToList(),
                AvailableWeeks = Enumerable.Range(1, 53).ToList(),
                WeeklySchedule = new Dictionary<DateTime, List<ManagerScheduleSlot>>()
            };

            // Tải các tùy chọn bộ lọc
            await LoadFilterOptions(model);

            // Tải lịch mặc định nếu không có lỗi bộ lọc
            try
            {
                model.WeeklySchedule = await _managerService.GetWeeklyScheduleAsync(
                    model.DepartmentId,
                    model.RoomId,
                    model.DoctorId,
                    model.Year,
                    model.Week
                );
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải lịch làm việc: {ex.Message}";
            }

            return View("~/Views/Manager/ScheduleOverview.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportSchedule(string format, Guid? departmentId, Guid? roomId, Guid? doctorId, int? year, int? week)
        {
            try
            {
                if (!year.HasValue || !week.HasValue)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn năm và tuần để xuất lịch.";
                    return RedirectToAction("ScheduleOverview", new { departmentId, roomId, doctorId, year, week });
                }

                var schedule = await _managerService.GetWeeklyScheduleAsync(departmentId, roomId, doctorId, year.Value, week.Value);
                if (format?.ToLower() == "excel")
                {
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add($"Lịch Tuần {week} Năm {year}");
                    var row = 1;

                    // Tiêu đề
                    worksheet.Cell(row, 1).Value = "Ca làm việc";
                    var col = 2;
                    var jan1 = new DateTime(year.Value, 1, 1);
                    var daysOffset = (int)DayOfWeek.Monday - (int)jan1.DayOfWeek;
                    if (daysOffset > 0) daysOffset -= 7;
                    var firstMonday = jan1.AddDays(daysOffset);
                    var startOfWeek = firstMonday.AddDays((week.Value - 1) * 7);

                    for (int i = 0; i < 7; i++)
                    {
                        var date = startOfWeek.AddDays(i);
                        worksheet.Cell(row, col).Value = $"{date:dd/MM/yyyy} ({date:dddd})";
                        col++;
                    }
                    row++;

                    // Dữ liệu - Ca sáng
                    worksheet.Cell(row, 1).Value = "Ca sáng";
                    col = 2;
                    for (int i = 0; i < 7; i++)
                    {
                        var date = startOfWeek.AddDays(i);
                        var morningSlots = schedule.ContainsKey(date.Date)
                            ? schedule[date.Date].Where(s => s.Period == "sáng").ToList()
                            : new List<ManagerScheduleSlot>();

                        var cellContent = morningSlots.Any()
                            ? string.Join("\n", morningSlots.Select(s => $"{s.DoctorName} - {s.RoomName} ({s.AppointmentCount}/{s.MaxAppointments})"))
                            : "Chưa có ca";
                        worksheet.Cell(row, col).Value = cellContent;
                        col++;
                    }
                    row++;

                    // Dữ liệu - Ca chiều
                    worksheet.Cell(row, 1).Value = "Ca chiều";
                    col = 2;
                    for (int i = 0; i < 7; i++)
                    {
                        var date = startOfWeek.AddDays(i);
                        var afternoonSlots = schedule.ContainsKey(date.Date)
                            ? schedule[date.Date].Where(s => s.Period == "chiều").ToList()
                            : new List<ManagerScheduleSlot>();

                        var cellContent = afternoonSlots.Any()
                            ? string.Join("\n", afternoonSlots.Select(s => $"{s.DoctorName} - {s.RoomName} ({s.AppointmentCount}/{s.MaxAppointments})"))
                            : "Chưa có ca";
                        worksheet.Cell(row, col).Value = cellContent;
                        col++;
                    }

                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Schedule_Week{week}_{year}.xlsx");
                }
                else if (format?.ToLower() == "pdf")
                {
                    TempData["ErrorMessage"] = "Chức năng xuất PDF chưa được triển khai.";
                    return RedirectToAction("ScheduleOverview", new { departmentId, roomId, doctorId, year, week });
                }

                TempData["ErrorMessage"] = "Định dạng xuất không hợp lệ.";
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
                // Tải danh sách khoa
                var departmentsResult = await _departmentService.GetDepartmentsAsync();
                model.Departments = departmentsResult.Success && departmentsResult.Data != null
                    ? departmentsResult.Data.Select(d => new Models.Appointment.DepartmentOption
                    {
                        Id = d.Id,
                        Name = d.Name
                    }).ToList()
                    : new List<Models.Appointment.DepartmentOption>();

                // Tải danh sách phòng nếu có DepartmentId
                if (model.DepartmentId.HasValue && model.DepartmentId != Guid.Empty)
                {
                    var roomsResult = await _departmentService.GetRoomsByDepartmentAsync(model.DepartmentId.Value);
                    model.Rooms = roomsResult.Success && roomsResult.Data != null
                        ? roomsResult.Data.Select(r => new Models.Manager.RoomOption
                        {
                            Id = r.Id,
                            Name = r.Name,
                            DepartmentId = r.DepartmentId
                        }).ToList()
                        : new List<Models.Manager.RoomOption>();
                }
                else
                {
                    model.Rooms = new List<Models.Manager.RoomOption>();
                }

                // Tải danh sách bác sĩ
                var doctorsResult = await _doctorService.GetAllDoctorsAsync();
                model.Doctors = doctorsResult.Success && doctorsResult.Data != null
                    ? doctorsResult.Data.Select(d => new Models.Manager.DoctorOption
                    {
                        Id = d.Id,
                        Name = d.FullName,
                        DepartmentId = d.Departments != null && d.Departments.Any() && Guid.TryParse(d.Departments.First(), out var deptId)
                            ? deptId
                            : Guid.Empty
                    }).ToList()
                    : new List<Models.Manager.DoctorOption>();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải tùy chọn bộ lọc: {ex.Message}";
                model.Departments = new List<Models.Appointment.DepartmentOption>();
                model.Rooms = new List<Models.Manager.RoomOption>();
                model.Doctors = new List<Models.Manager.DoctorOption>();
            }
        }

        private int GetWeekOfYear(DateTime date)
        {
            var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
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
        #endregion
    }
}