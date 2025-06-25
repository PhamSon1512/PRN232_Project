using MediAppointment.Client.Models.Doctor;
using MediAppointment.Client.Services;
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

        [HttpGet]
        public async Task<IActionResult> Profile(Guid id)
        {
            var result = await _doctorService.GetDoctorProfileAsync(id);
            
            if (result.Success && result.Data != null)
            {
                return View(result.Data);
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tải thông tin bác sĩ";
            return RedirectToAction("Index");
        }
    }
}
