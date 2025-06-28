namespace MediAppointment.Application.DTOs
{
    public class DoctorUpdateDto
    {
        public string? FullName { get; set; } = default!;
        public bool? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = default!;
        
    }
}
