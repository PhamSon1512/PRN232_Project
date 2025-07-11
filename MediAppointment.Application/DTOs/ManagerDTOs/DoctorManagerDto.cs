namespace MediAppointment.Application.DTOs.ManagerDTOs
{
    public class DoctorManagerDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Role { get; set; } = default!;
        public List<string> Departments { get; set; } = new List<string>();
    }
}
