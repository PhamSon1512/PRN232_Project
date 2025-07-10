namespace MediAppointment.Application.DTOs.ManagerDTOs
{
    public class ManagerCreateDto
    {
        public Guid DoctorId { get; set; }
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string? NewRole { get; set; }
    }
}
