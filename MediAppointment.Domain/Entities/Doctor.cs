namespace MediAppointment.Domain.Entities
{
    public class Doctor
    {
        public int DoctorID { get; set; }
        public int UserID { get; set; }
        public int Status { get; set; }
        public string? Description { get; set; }

        public ICollection<DoctorDepartment>? DoctorDepartments { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Schedule>? Schedules { get; set; }
        public ICollection<MedicalRecord>? MedicalRecords { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
    }
}
