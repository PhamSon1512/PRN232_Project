namespace MediAppointment.Application.DTOs.ManagerDTOs
{
    public class ManagerUpdateDto
    {
        public Guid DoctorId { get; set; }
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
    }
    public class AdminUpdateProfileDto
    {
        public Guid AdminId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
