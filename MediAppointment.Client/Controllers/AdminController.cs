using System.Security.Claims;
using System.Text.RegularExpressions;
using MediAppointment.Client.Attributes;
using MediAppointment.Client.Models.Admin;
using MediAppointment.Client.Models.Common;
using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.Client.Controllers
{
    [RequireAdmin]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [Route("Admin")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [Route("Admin/UserManagement")]
        public async Task<IActionResult> UserManagement(int page = 1, int pageSize = 5, string text = "")
        {
            var apiResponse = await _adminService.GetAllUsersAsync(page, pageSize, text);
            if (!apiResponse.Success)
            {
                ViewBag.ErrorMessage = apiResponse.ErrorMessage ?? "Không thể tải danh sách người dùng.";
                return View();
            }
            return View(apiResponse.Data);
        }

        [Route("Admin/EditUser/{id}")]
        [HttpGet]
        public async Task<IActionResult> EditUser(Guid id)
        {
            var apiResponse = await _adminService.GetUserByIdAsync(id);
            if (!apiResponse.Success || apiResponse.Data == null)
            {
                return View("EditUser", new AdminViewModel { ErrorMessage = apiResponse.ErrorMessage ?? "Không thể tải thông tin người dùng." });
            }
            return View("EditUser", apiResponse.Data);
        }

        [HttpPost]
        [Route("Admin/EditUser/{id}")]
        public async Task<IActionResult> EditUser(Guid id, [Bind("FullName,PhoneNumber,Role")] EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await _adminService.GetUserByIdAsync(id);
                var viewModel = user.Success && user.Data != null ? user.Data : new AdminViewModel();
                viewModel.ErrorMessage = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();
                return View(viewModel);
            }

            var userResponse = await _adminService.GetUserByIdAsync(id);
            if (!userResponse.Success || userResponse.Data == null)
            {
                return View(new AdminViewModel { ErrorMessage = "Không tìm thấy người dùng." });
            }

            var originalUser = userResponse.Data;

            if (string.IsNullOrEmpty(model.Role) || model.Role == originalUser.Role)
            {
                var dto = new ManagerUpdateDto
                {
                    DoctorId = id,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber
                };

                var updateResponse = await _adminService.UpdateManagerProfileAsync(dto);
                if (updateResponse.Success)
                {
                    return RedirectToAction("UserManagement");
                }
                originalUser.ErrorMessage = updateResponse.ErrorMessage ?? "Cập nhật thất bại.";
            }
            else
            {
                if (originalUser.Role != "Doctor" || model.Role != "Manager")
                {
                    originalUser.ErrorMessage = "Chỉ được nâng cấp từ Doctor thành Manager.";
                    return View(originalUser);
                }

                var dto = new ManagerCreateDto
                {
                    DoctorId = id,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber
                };

                var updateResponse = await _adminService.UpgradeToManagerAsync(dto);
                if (updateResponse.Success)
                {
                    return RedirectToAction("UserManagement");
                }
                originalUser.ErrorMessage = updateResponse.ErrorMessage ?? "Nâng cấp thất bại.";
            }

            return View(originalUser);
        }

        [HttpGet("AdminProfile")]
        public async Task<IActionResult> AdminProfile()
        {
            var result = await _adminService.GetAdminProfileAsync();
            if (result.Success && result.Data != null)
            {
                return View("~/Views/Account/AdminProfile.cshtml", result.Data);
            }
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải thông tin admin";
            return RedirectToAction("Dashboard");
        }

        [HttpGet("EditAdminProfile")]
        public async Task<IActionResult> EditAdminProfile()
        {
            var result = await _adminService.GetAdminProfileAsync();
            if (result.Success && result.Data != null)
            {
                return View(result.Data);
            }
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải thông tin admin";
            return RedirectToAction("AdminProfile");
        }

        [HttpPost("UpdateAdminProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAdminProfile(AdminUpdateProfile dto)
        {
            var result = await _adminService.GetAdminProfileAsync();
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
                if (result.Success && result.Data != null)
                {
                    return View("EditAdminProfile", result.Data);
                }
                return RedirectToAction("AdminProfile");
            }

            // Gán Id từ token để đảm bảo không dùng GUID mặc định
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var adminId))
            {
                dto.Id = adminId;
            }
            else
            {
                // Log lỗi nếu không parse được
                Console.WriteLine($"Failed to parse userIdClaim: {userIdClaim}");
                TempData["ErrorMessage"] = "Không thể xác định ID admin.";
                return RedirectToAction("AdminProfile");
            }

            var resultUpdate = await _adminService.UpdateAdminProfileAsync(dto);
            if (resultUpdate.Success)
            {
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("AdminProfile");
            }

            TempData["ErrorMessage"] = resultUpdate.ErrorMessage ?? "Cập nhật hồ sơ thất bại";
            if (result.Success && result.Data != null)
            {
                return View("EditAdminProfile", result.Data);
            }
            return RedirectToAction("AdminProfile");
        }
    }
}