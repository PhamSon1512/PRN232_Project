using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
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
        public async Task<IActionResult> Dashboard()
        {
            var statsResponse = await _adminService.GetDashboardStatsAsync();
            if (statsResponse.Success && statsResponse.Data != null)
            {
                return View("~/Views/Admin/Dashboard.cshtml", statsResponse.Data);
            }
            TempData["ErrorMessage"] = statsResponse.ErrorMessage ?? "Không thể tải thống kê dashboard.";
            return View("~/Views/Admin/Dashboard.cshtml", new DashboardViewModel());
        }

        [HttpGet("Admin/UserManagement")]
        public async Task<IActionResult> UserManagement(int page = 1, int pageSize = 5, string text = "")
        {
            var apiResponse = await _adminService.GetAllUsersAsync(page, pageSize, text);
            if (apiResponse.Success && apiResponse.Data != null)
            {
                ViewBag.Text = text;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                return View("~/Views/Admin/UserManagement.cshtml", apiResponse.Data);
            }
            TempData["ErrorMessage"] = apiResponse.ErrorMessage ?? "Không thể tải danh sách người dùng.";
            return View("~/Views/Admin/UserManagement.cshtml");
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
        public async Task<IActionResult> EditUser(Guid id, [Bind("FullName,PhoneNumber,Role,IsActive")] EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await _adminService.GetUserByIdAsync(id);
                var viewModel = user.Success && user.Data != null ? user.Data : new AdminViewModel();
                viewModel.FullName = model.FullName;
                viewModel.PhoneNumber = model.PhoneNumber;
                viewModel.Role = model.Role;
                viewModel.IsActive = model.IsActive;
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
                    PhoneNumber = model.PhoneNumber,
                    IsActive = model.IsActive
                };
                var updateResponse = await _adminService.UpdateManagerProfileAsync(dto);
                if (updateResponse.Success)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin người dùng thành công!";
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
                    PhoneNumber = model.PhoneNumber,
                    IsActive = model.IsActive
                };
                var updateResponse = await _adminService.UpgradeToManagerAsync(dto);
                if (updateResponse.Success)
                {
                    TempData["SuccessMessage"] = "Nâng cấp người dùng thành Manager thành công!";
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
            if (!ModelState.IsValid)
            {
                var result = await _adminService.GetAdminProfileAsync();
                if (result.Success && result.Data != null)
                {
                    result.Data.ErrorMessage = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .FirstOrDefault();
                    return View("EditAdminProfile", result.Data);
                }
                TempData["ErrorMessage"] = "Không thể tải thông tin admin";
                return RedirectToAction("AdminProfile");
            }

            // Lấy adminId từ session
            var adminId = GetAdminIdFromSession();
            if (adminId == null)
            {
                TempData["ErrorMessage"] = "Không thể xác định ID admin từ session.";
                return RedirectToAction("AdminProfile");
            }

            dto.Id = adminId.Value;

            var resultUpdate = await _adminService.UpdateAdminProfileAsync(dto);
            if (resultUpdate.Success)
            {
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("AdminProfile");
            }

            TempData["ErrorMessage"] = resultUpdate.ErrorMessage ?? "Cập nhật hồ sơ thất bại";
            var resultProfile = await _adminService.GetAdminProfileAsync();
            if (resultProfile.Success && resultProfile.Data != null)
            {
                return View("EditAdminProfile", resultProfile.Data);
            }
            return RedirectToAction("AdminProfile");
        }

        private Guid? GetAdminIdFromSession()
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
                var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;

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
    }
}