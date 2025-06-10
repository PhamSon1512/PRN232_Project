using MediAppointment.Domain.Entities;

namespace MediAppointment.Domain.Entities
{
    public class DoctorDepartment
    {
        public Guid DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        public Guid DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}

