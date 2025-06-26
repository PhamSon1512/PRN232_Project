using MediAppointment.Domain.Enums;

namespace MediAppointment.Application.DTOs
{
    public class ManagerDoctorUpdateDTO
    {
        public Guid UserIdentityId { get; set; }
        public List<Guid> Departments { get; set; } = new List<Guid>();
        public Status Status { get; set; }
    }
}
