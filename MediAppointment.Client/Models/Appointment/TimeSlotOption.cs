namespace MediAppointment.Client.Models.Appointment
{
    public class TimeSlotOption
    {
        public Guid Id { get; set; }
        public string TimeRange { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public DateTime Date { get; set; }
        public string Period { get; set; } = string.Empty; // "morning" or "afternoon"
        public int AvailableRooms { get; set; }
        public int TotalRooms { get; set; }
        public bool Shift { get; set; } // false = sáng, true = chiều
    }
}
