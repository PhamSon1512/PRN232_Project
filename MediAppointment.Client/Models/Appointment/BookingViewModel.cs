using System.ComponentModel.DataAnnotations;

namespace MediAppointment.Client.Models.Appointment
{
    public class BookingViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        public Guid DepartmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày khám")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Vui lòng chọn giờ khám")]
        public Guid TimeSlotId { get; set; }

        public string? Note { get; set; }

        // For UI
        public List<DepartmentOption> Departments { get; set; } = new();
        public List<TimeSlotOption> AvailableTimeSlots { get; set; } = new();
        public bool ShowTimeSlots { get; set; } = false;
    }
}
