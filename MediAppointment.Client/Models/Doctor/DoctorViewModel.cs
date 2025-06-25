namespace MediAppointment.Client.Models.Doctor
{
    public class DoctorViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string GenderDisplay => Gender ? "Nam" : "Nữ";
        public List<string> Departments { get; set; } = new();
        public List<ScheduleViewModel> Schedules { get; set; } = new();
    }

    public class ScheduleViewModel
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Shift { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }
}
