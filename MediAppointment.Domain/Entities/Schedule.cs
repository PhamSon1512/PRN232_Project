using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Enums;

namespace MediAppointment.Domain.Entities
{
    public class Schedule : Entity
    {
        public Guid DoctorId { get; set; }
        public System.DayOfWeek DayOfWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<Shift>? Shifts { get; set; } 
    }
}