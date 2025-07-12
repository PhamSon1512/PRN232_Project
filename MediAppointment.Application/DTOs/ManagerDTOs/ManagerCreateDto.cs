namespace MediAppointment.Application.DTOs.ManagerDTOs
{
    public class ManagerCreateDto
    {
        public Guid DoctorId { get; set; }
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string? NewRole { get; set; }
    }
    public class ManagerProfileDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
