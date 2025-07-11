using MediAppointment.Client.Models.Auth;
using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.Client.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var loginResult = await _authService.LoginAsync(model);

            if (loginResult != null && loginResult.Success)
            {
                Response.Cookies.Append("AccessToken", loginResult.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

                HttpContext.Session.SetString("UserId", loginResult.UserId ?? "");
                HttpContext.Session.SetString("UserRole", loginResult.Role ?? "");
                HttpContext.Session.SetString("IsAuthenticated", "true");

                // ✅ Chuyển hướng theo vai trò
                switch (loginResult.Role)
                {
                    case "Doctor":
                        return RedirectToAction("Index", "DoctorHome");
                    case "Admin":
                        return RedirectToAction("Index", "Admin");
                    case "Manager":
                        return RedirectToAction("Index", "Manager");
                    default:
                        return RedirectToAction("Index", "Home"); // fallback nếu role không xác định
                }
            }

            ModelState.AddModelError("", loginResult?.ErrorMessage ?? "Đăng nhập thất bại");
            return View(model);
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RegisterAsync(model);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Đăng ký thất bại");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            TempData["InfoMessage"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> CheckAuth()
        {
            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            return Json(new { isAuthenticated });
        }
    }
}
