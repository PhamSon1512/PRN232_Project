namespace MediAppointment.Application.DTOs.DoctorDTOs
{
    public class DoctorDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public int Status { get; set; } = 1; // 0: Inactive, 1: Active, 2: Pending, 3: Deleted
        public List<string> Departments { get; set; } = new List<string>();
    }
}
