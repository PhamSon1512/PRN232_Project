namespace MediAppointment.Application.DTOs.ManagerDTOs
{
    public class ManagerUpdateDto
    {
        public Guid DoctorId { get; set; }
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
    }
    public class ManagerUpdateProfileDto
    {
        public Guid ManagerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class AdminUpdateProfileDto
    {
        public Guid? AdminId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
