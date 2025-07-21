namespace MediAppointment.Application.DTOs.ManagerDTOs
{
    public class ManagerUpdateDto
    {
        public Guid DoctorId { get; set; }
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public bool? IsActive { get; set; }
    }
    public class ManagerUpdateProfileDto
    {
        public Guid ManagerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    // ADMIN
    public class AdminUpdateProfileDto
    {
        public Guid? AdminId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }
    public class DashboardDto
    {
        public int TotalDoctors { get; set; }
        public int TotalManagers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
