namespace MediAppointment.Application.DTOs.AdminDTOs
{
    public class AdminDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Status { get; set; } = default!;
    }
}
