namespace MediAppointment.Domain.Entities
{
    public class Notification
    {
        public int UserID { get; set; }
        public int DoctorID { get; set; }
        public int NotifyID { get; set; }
        public string? Content { get; set; }
        public int Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public Doctor? Doctor { get; set; }
    }
}
