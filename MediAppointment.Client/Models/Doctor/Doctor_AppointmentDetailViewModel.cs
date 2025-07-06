namespace MediAppointment.Client.Models.Doctor
{
    public class Doctor_AppointmentDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RoomName { get; set; } = "";
        public string Time { get; set; } = "";
    }
}
