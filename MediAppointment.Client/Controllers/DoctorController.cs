using System.Text.RegularExpressions;
using MediAppointment.Client.Attributes;
using MediAppointment.Client.Models.Doctor;
using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.Client.Controllers
{
    public class DoctorController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IAuthService _authService;

        public DoctorController(IDoctorService doctorService, IAuthService authService)
        {
            _doctorService = doctorService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _doctorService.GetAllDoctorsAsync();
            
            if (result.Success)
            {
                return View(result.Data ?? new List<DoctorViewModel>());
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải danh sách bác sĩ";
            return View(new List<DoctorViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> Profile(Guid doctorId)
        {
            var result = await _doctorService.GetDoctorProfileAsync(doctorId);

            if (result.Success && result.Data != null)
            {
                return View(result.Data);
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải thông tin bác sĩ";
            return RedirectToAction("Index");
        }

        [RequireDoctor]
        [HttpGet("DoctorProfile")]
        public async Task<IActionResult> DoctorProfile()
        {
            var result = await _doctorService.GetLoggedInDoctorProfileAsync();

            if (result.Success && result.Data != null)
            {
                return View("~/Views/Account/DoctorProfile.cshtml", result.Data);
            }

            System.Diagnostics.Debug.WriteLine($"Error fetching doctor profile: {result.ErrorMessage}");
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải thông tin bác sĩ";
            return RedirectToAction("Index", "DoctorHome");
        }

        [RequireDoctor]
        [HttpGet("EditProfile")]
        public async Task<IActionResult> EditProfile()
        {
            var result = await _doctorService.GetLoggedInDoctorProfileAsync();
            if (result.Success && result.Data != null)
            {
                var model = new DoctorStatusModel
                {
                    Id = result.Data.Id,
                    FullName = result.Data.FullName,
                    Gender = result.Data.Gender,
                    DateOfBirth = result.Data.DateOfBirth,
                    Email = result.Data.Email,
                    PhoneNumber = result.Data.PhoneNumber,
                    Departments = result.Data.Departments,
                    Schedules = result.Data.Schedules,
                    Status = result.Data.Status
                };
                return View(model);
            }
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải thông tin bác sĩ";
            return RedirectToAction("DoctorProfile");
        }

        [RequireDoctor]
        [HttpPost("UpdateProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(DoctorUpdateProfile dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
            {
                ModelState.AddModelError("FullName", "Họ và tên không được để trống");
            }
            else if (!Regex.IsMatch(dto.FullName, @"^[a-zA-ZÀ-ỹ\s]+$"))
            {
                ModelState.AddModelError("FullName", "Họ và tên chỉ được chứa chữ cái và khoảng trắng");
            }

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại không được để trống");
            }
            else if (!Regex.IsMatch(dto.PhoneNumber, @"^\d+$") || dto.PhoneNumber.Length > 11)
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại phải là số và có độ dài tối đa 11 ký tự");
            }

            if (!ModelState.IsValid)
            {
                var result = await _doctorService.GetLoggedInDoctorProfileAsync();
                if (result.Success && result.Data != null)
                {
                    var model = new DoctorStatusModel
                    {
                        Id = result.Data.Id,
                        FullName = dto.FullName ?? result.Data.FullName,
                        Gender = result.Data.Gender,
                        DateOfBirth = result.Data.DateOfBirth,
                        Email = result.Data.Email,
                        PhoneNumber = dto.PhoneNumber ?? result.Data.PhoneNumber,
                        Departments = result.Data.Departments,
                        Schedules = result.Data.Schedules,
                        Status = result.Data.Status
                    };
                    return View("EditProfile", model);
                }
                return RedirectToAction("DoctorProfile");
            }

            var resultUpdate = await _doctorService.UpdateDoctorProfileAsync(dto);
            if (resultUpdate.Success)
            {
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("DoctorProfile");
            }

            TempData["ErrorMessage"] = resultUpdate.ErrorMessage ?? "Cập nhật hồ sơ thất bại";
            var resultProfile = await _doctorService.GetLoggedInDoctorProfileAsync();
            if (resultProfile.Success && resultProfile.Data != null)
            {
                return View("EditProfile", new DoctorStatusModel
                {
                    Id = resultProfile.Data.Id,
                    FullName = dto.FullName ?? resultProfile.Data.FullName,
                    Gender = resultProfile.Data.Gender,
                    DateOfBirth = resultProfile.Data.DateOfBirth,
                    Email = resultProfile.Data.Email,
                    PhoneNumber = dto.PhoneNumber ?? resultProfile.Data.PhoneNumber,
                    Departments = resultProfile.Data.Departments,
                    Schedules = resultProfile.Data.Schedules,
                    Status = resultProfile.Data.Status
                });
            }
            return RedirectToAction("DoctorProfile");
        }
    }
}
