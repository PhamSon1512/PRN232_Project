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

    public class DoctorStatusModel : DoctorViewModel
    {
        public int Status { get; set; }
    }

    public class DoctorCreateModel
    {
        public string FullName { get; set; } = string.Empty;
        public bool? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<Guid> Departments { get; set; } = new List<Guid>();
    }

    public class DoctorUpdateModel
    {
        public int Status { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public List<Guid> Departments { get; set; } = new List<Guid>();
    }

    public class DoctorUpdateProfile
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class ScheduleViewModel
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Shift { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }

    public class Doctor_SlotListViewModel
    {
        public DateTime Date { get; set; }
        public string Shift { get; set; } = "";
        public List<Doctor_AssignedSlotViewModel> Slots { get; set; } = new();

        public int SelectedYear { get; set; }
        public string SelectedWeek { get; set; } = "";
    }
}
