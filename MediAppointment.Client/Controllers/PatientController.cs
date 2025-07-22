using MediAppointment.Client.Models.MedicalRecord;
using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.Client.Controllers
{
    public class PatientController : Controller
    {
        private readonly IPatientProfileApiService _patientService;

        public PatientController(IPatientProfileApiService patientService)
        {
            _patientService = patientService;
        }

        // GET: /Patient/Profile
        public async Task<IActionResult> Profile()
        {
            var profile = await _patientService.GetPatientProfileAsync();
            if (profile == null)
            {
                TempData["Error"] = "Không thể lấy thông tin hồ sơ bệnh nhân.";
                return RedirectToAction("Index", "Home");
            }
            return View(profile);
        }

        // POST: /Patient/AddMedicalRecord
        [HttpPost]
        public async Task<IActionResult> AddMedicalRecord(CreateMedicalRecordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _patientService.AddMedicalRecordAsync(request);

            if (result)
            {
                TempData["Success"] = "Thêm hồ sơ bệnh án thành công!";
                return RedirectToAction("Profile");
            }

            TempData["Error"] = "Thêm hồ sơ bệnh án thất bại.";
            return View(request);
        }
    }
}
