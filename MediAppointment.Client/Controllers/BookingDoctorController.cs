using MediAppointment.Client.Attributes;
using Microsoft.AspNetCore.Mvc;
using MediAppointment.Client.Services;
using MediAppointment.Client.Models.Appointment;

namespace MediAppointment.Client.Controllers
{
    [RequirePatient]
    public class BookingDoctorController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly IDoctorService _doctorService;
        private readonly IBookingDoctorService _bookingDoctorService;
        public BookingDoctorController(IAppointmentService appointmentService, ITimeSlotService timeSlotService, IDoctorService doctorService, IBookingDoctorService bookingDoctorService)
        {
            _appointmentService = appointmentService;
            _timeSlotService = timeSlotService;
            _doctorService = doctorService;
            _bookingDoctorService = bookingDoctorService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new BookingDoctorViewModel();

            // Load departments
            var departmentsResult = await _appointmentService.GetDepartmentsAsync();
            if (departmentsResult.Success && departmentsResult.Data != null)
            {
                model.Departments = departmentsResult.Data;
            }

            // Load doctors
            var doctorsResult = await _doctorService.GetAllDoctorsAsync();
            if (doctorsResult.Success && doctorsResult.Data != null)
            {
                model.Doctors = doctorsResult.Data;
            }

            // Load time slots
            var timeSlotsResult = await _timeSlotService.GetTimeSlotsAsync();
            if (timeSlotsResult.Success && timeSlotsResult.Data != null)
            {
                model.TimeSlots = timeSlotsResult.Data;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(BookingDoctorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownData(model);
                return View(model);
            }

            var request = new BookingDoctorCreate
            {
                DoctorId = model.DoctorId.Value,
                DepartmentId = model.RoomId.Value,
                TimeSlotId = model.TimeSlotId.Value,
                AppointmentDate = model.AppointmentDate.Value,
                Note = model.Note
            };

            var result = await _bookingDoctorService.BookAppointmentAsync(request);

            if (!result)
            {
                ModelState.AddModelError("", "Đặt lịch không thành công. Vui lòng thử lại.");
                await LoadDropdownData(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Đặt lịch thành công!";
            return RedirectToAction("Index");
        }


        private async Task LoadDropdownData(BookingDoctorViewModel model)
        {
            var departmentsResult = await _appointmentService.GetDepartmentsAsync();
            if (departmentsResult.Success && departmentsResult.Data != null)
                model.Departments = departmentsResult.Data;

            var doctorsResult = await _doctorService.GetAllDoctorsAsync();
            if (doctorsResult.Success && doctorsResult.Data != null)
                model.Doctors = doctorsResult.Data;

            var timeSlotsResult = await _timeSlotService.GetTimeSlotsAsync();
            if (timeSlotsResult.Success && timeSlotsResult.Data != null)
                model.TimeSlots = timeSlotsResult.Data;
        }


    }
}
