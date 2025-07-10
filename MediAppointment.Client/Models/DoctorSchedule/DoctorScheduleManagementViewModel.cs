using System.ComponentModel.DataAnnotations;
using MediAppointment.Client.Models.Appointment;

namespace MediAppointment.Client.Models.DoctorSchedule
{
    public class DoctorScheduleManagementViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        public Guid DepartmentId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn phòng")]
        public Guid RoomId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn năm")]
        public int Year { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn tuần")]
        public int Week { get; set; }
        
        public List<DepartmentOption> Departments { get; set; } = new();
        public List<RoomOption> Rooms { get; set; } = new();
        public List<int> AvailableYears { get; set; } = new();
        public List<int> AvailableWeeks { get; set; } = new();
        public Dictionary<DateTime, List<ScheduleSlot>> WeeklySchedule { get; set; } = new();
        public List<ScheduleCreateRequest> SelectedSlots { get; set; } = new();
    }

    public class RoomOption
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
    }

    public class ScheduleSlot
    {
        public Guid Id { get; set; }
        public string TimeRange { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public bool IsOccupied { get; set; }
        public DateTime Date { get; set; }
        public string Period { get; set; } = string.Empty; // "morning" or "afternoon"
    }

    public class ScheduleCreateRequest
    {
        public Guid RoomId { get; set; }
        public DateTime Date { get; set; }
        public string Period { get; set; } = string.Empty;
        public List<Guid> TimeSlotIds { get; set; } = new();
    }

    public class ScheduleDeleteRequest
    {
        public Guid RoomId { get; set; }
        public DateTime Date { get; set; }
        public string Period { get; set; } = string.Empty;
    }

    public class BulkScheduleRequest
    {
        public List<ScheduleCreateRequest> Schedules { get; set; } = new();
    }

    public class BulkDeleteRequest
    {
        public List<ScheduleDeleteRequest> Schedules { get; set; } = new();
    }
}
