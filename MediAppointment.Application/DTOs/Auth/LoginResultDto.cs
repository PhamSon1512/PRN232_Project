namespace MediAppointment.Application.DTOs.Auth
{
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid? UserId { get; set; }
        public string? Role { get; set; }
    }
}
