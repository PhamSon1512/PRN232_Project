namespace MediAppointment.Application.DTOs
{
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid? UserId { get; set; }
        public string? Role { get; set; }
    }
}
