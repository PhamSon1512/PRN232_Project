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

        // Không nên update Password ? ?ây
        // Thêm các tr??ng ??c thù cho Doctor n?u có
    }
}
