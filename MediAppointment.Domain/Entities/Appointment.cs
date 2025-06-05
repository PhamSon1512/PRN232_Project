namespace MediAppointment.Domain.Entities
{
    public class Appointment
    {
        public int AppointmentID { get; set; }
        public int DoctorID { get; set; }
        public int DepartmentID { get; set; }
        public int UserID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public Doctor? Doctor { get; set; }
        public Department? Department { get; set; }
    }
}
