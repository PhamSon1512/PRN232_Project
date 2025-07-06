namespace MediAppointment.Client.Models.Doctor
{
    public class Doctor_SlotAppointmentViewModel
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public int Status { get; set; }
        public string DoctorName { get; set; } = "";
        public string RoomName { get; set; } = "";
        public string TimeStart { get; set; } = "";
        public string TimeEnd { get; set; } = "";
        public string Duration { get; set; } = "";
        public string Shift { get; set; } = "";

        // Chỉ 1 appointment duy nhất
        public Doctor_AppointmentDetailViewModel? Appointment { get; set; }
    }
}
