namespace MediAppointment.Client.Models.Appointment
{
    public class BookingDoctorStatusUpdate
    {
        public string Status { get; set; } = "Pending"; // "Approved" hoặc "Rejected"
        public string? Note { get; set; }
        public Guid TimeSlotID { get; set; }
        public Guid PatientID { get; set; }
        public Guid DepartmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}
