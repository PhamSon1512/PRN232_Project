namespace MediAppointment.Domain.Entities
{
    public class DoctorDepartment
    {
        public int DoctorID { get; set; }
        public int DepartmentID { get; set; }
        public Doctor? Doctor { get; set; }
        public Department? Department { get; set; }
    }
}
