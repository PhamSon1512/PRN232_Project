using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Enums;

namespace MediAppointment.Domain.Entities
{
    public class Appointment : Entity
    {
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid DepartmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}
