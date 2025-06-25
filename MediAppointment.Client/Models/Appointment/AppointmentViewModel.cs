using System.ComponentModel.DataAnnotations;

namespace MediAppointment.Client.Models.Appointment
{
    public class AppointmentViewModel
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid RoomTimeSlotId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        
        public string StatusDisplay => Status switch
        {
            "Scheduled" => "Đã đặt lịch",
            "Completed" => "Hoàn thành",
            "Cancelled" => "Đã hủy",
            "Rescheduled" => "Đã dời lịch",
            "Pending" => "Chờ xác nhận",
            _ => Status
        };

        public string StatusClass => Status switch
        {
            "Scheduled" => "badge bg-primary",
            "Completed" => "badge bg-success",
            "Cancelled" => "badge bg-danger",
            "Rescheduled" => "badge bg-warning",
            "Pending" => "badge bg-secondary",
            _ => "badge bg-light"
        };
    }

    public class CreateAppointmentViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
        [Display(Name = "Bác sĩ")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày khám")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày khám")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khung giờ")]
        [Display(Name = "Khung giờ")]
        public Guid RoomTimeSlotId { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Note { get; set; }

        public List<DoctorOptionViewModel> AvailableDoctors { get; set; } = new();
        public List<TimeSlotOptionViewModel> AvailableTimeSlots { get; set; } = new();
    }

    public class DoctorOptionViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class TimeSlotOptionViewModel
    {
        public Guid RoomTimeSlotId { get; set; }
        public string TimeRange { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }
}
