namespace MediAppointment.Client.Models.Doctor
{
    public class Doctor_PatientInfoViewModel
    {
        public Doctor_SlotAppointmentViewModel? AppointmentSlot { get; set; }
        public Doctor_PatientDetailViewModel Patient { get; set; }
        public Doctor_AppointmentDetailViewModel? AppointmentDetail { get; set; }
        public string? Week { get; set; }
        public int Year { get; set; }
        public DateTime Date { get; set; }
        public string? Period { get; set; }
    }
}
