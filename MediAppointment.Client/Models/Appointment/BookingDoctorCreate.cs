namespace MediAppointment.Client.Models.Appointment
{
    public class BookingDoctorCreate
    {
        public Guid DoctorId { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid TimeSlotId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Note { get; set; }
    }

}
