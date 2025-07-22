using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MediAppointment.Domain.Entities
{
    public class AppointmentBookingDoctor : Entity
    {
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid TimeSlotId { get; set; }

        public DateTime AppointmentDate { get; set; }
        public string? Note { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
