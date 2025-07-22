namespace MediAppointment.Client.Models.Appointment
{
    public class BookingDoctorStatusUpdate
    {
        public string Status { get; set; } = "Pending"; // "Approved" hoặc "Rejected"
        public string? Note { get; set; }
    }
}
