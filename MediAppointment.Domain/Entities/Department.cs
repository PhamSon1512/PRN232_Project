using MediAppointment.Domain.Entities.Abstractions;

namespace MediAppointment.Domain.Entities
{
    public class Department : Entity
    {
        public string DepartmentName { get; set; }
        public ICollection<DoctorDepartment>? DoctorDepartments { get; set; }
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
