namespace MediAppointment.Application.DTOs
{
    public class DoctorCreateDto
    {
        public string FullName { get; set; } = default!;
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
        // Th�m c�c tr??ng ??c th� cho Doctor n?u c�
    }
}
