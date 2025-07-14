namespace MediAppointment.Client.Models.Appointment
{
    public class DepartmentOption
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalRooms { get; set; }
    }
}
