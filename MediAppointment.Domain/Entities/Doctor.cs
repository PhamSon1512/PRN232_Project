using MediAppointment.Domain.Entities.Abstractions;

namespace MediAppointment.Domain.Entities
{
    public class Doctor : User
    {
        public ICollection<DoctorDepartment>? DoctorDepartments { get; set; }
        public ICollection<Schedule>? Schedules { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
        public ICollection<RoomTimeSlot>? RoomTimeSlots { get; set; } 

    }
}
