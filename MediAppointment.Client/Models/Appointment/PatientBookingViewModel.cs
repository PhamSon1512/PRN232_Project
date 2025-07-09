using System.ComponentModel.DataAnnotations;

namespace MediAppointment.Client.Models.Appointment
{
    public class PatientBookingViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        public Guid DepartmentId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn năm")]
        public int Year { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn tuần")]
        public int Week { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn ngày")]
        public DateTime Date { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn khung giờ")]
        public Guid TimeSlotId { get; set; }
        
        public List<DepartmentOption> Departments { get; set; } = new();
        public List<int> AvailableYears { get; set; } = new();
        public List<int> AvailableWeeks { get; set; } = new();
        public List<TimeSlotOption> AvailableTimeSlots { get; set; } = new();
        public Dictionary<DateTime, List<TimeSlotOption>> WeeklyTimeSlots { get; set; } = new();
    }

    public class DepartmentOption
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TimeSlotOption
    {
        public Guid Id { get; set; }
        public string TimeRange { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public DateTime Date { get; set; }
        public string Period { get; set; } = string.Empty; // "morning" or "afternoon"
    }
}
