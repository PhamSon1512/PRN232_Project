using MediAppointment.Client.Models.Appointment;

namespace MediAppointment.Client.Models.Admin
{
    public class AdminScheduleOverviewViewModel
    {
        public Guid? DepartmentId { get; set; }
        public Guid? RoomId { get; set; }
        public Guid? DoctorId { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }
        
        public List<DepartmentOption> Departments { get; set; } = new();
        public List<RoomOption> Rooms { get; set; } = new();
        public List<DoctorOption> Doctors { get; set; } = new();
        public List<int> AvailableYears { get; set; } = new();
        public List<int> AvailableWeeks { get; set; } = new();
        public Dictionary<DateTime, List<AdminScheduleSlot>> WeeklySchedule { get; set; } = new();
    }

    public class DoctorOption
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
    }

    public class RoomOption
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
    }

    public class AdminScheduleSlot
    {
        public Guid Id { get; set; }
        public string TimeRange { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Period { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public int MaxAppointments { get; set; }
        public bool IsFullyBooked { get; set; }
    }
}
