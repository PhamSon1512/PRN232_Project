namespace MediAppointment.Application.DTOs
{
    public class PatientUpdateDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        // Kh�ng n�n update Password ? ?�y
        // Th�m c�c tr??ng ??c th� cho Patient n?u c�
    }
}
