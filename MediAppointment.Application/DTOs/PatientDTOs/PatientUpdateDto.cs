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
        // Không nên update Password ? ?ây
        // Thêm các tr??ng ??c thù cho Patient n?u có
    }
}
