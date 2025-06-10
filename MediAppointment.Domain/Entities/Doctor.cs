using MediAppointment.Domain.Entities.Abstractions;

namespace MediAppointment.Domain.Entities
{
    public class Doctor : User
    {
        public ICollection<DoctorDepartment>? DoctorDepartments { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Schedule>? Schedules { get; set; }
        public ICollection<Notification>? Notifications { get; set; }

    }
}
