namespace MediAppointment.Application.DTOs
{
    public class DoctorUpdateDto
    {
        public Guid UserIdentityId { get; set; }
        public string? FullName { get; set; } = default!;
        public bool? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; } = default!;
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }

        // Kh�ng n�n update Password ? ?�y
        // Th�m c�c tr??ng ??c th� cho Doctor n?u c�
    }
}
