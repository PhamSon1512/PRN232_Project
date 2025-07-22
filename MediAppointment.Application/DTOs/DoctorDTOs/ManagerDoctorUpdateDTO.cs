using MediAppointment.Domain.Enums;

namespace MediAppointment.Application.DTOs
{
    public class ManagerDoctorUpdateDTO
    {
        public string? FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public Status Status { get; set; }

        //public List<Guid> Departments { get; set; } = new List<Guid>();
    }
}
