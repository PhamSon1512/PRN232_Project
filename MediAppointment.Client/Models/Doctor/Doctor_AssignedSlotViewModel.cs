namespace MediAppointment.Client.Models.Doctor
{
    public class Doctor_AssignedSlotViewModel
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
    }
}

