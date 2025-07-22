namespace MediAppointment.Application.DTOs.ManagerDTOs
{
    public class ManagerScheduleSlot
    {
        public Guid Id { get; set; }
        public string TimeRange { get; set; }
        public string DoctorName { get; set; }
        public string RoomName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime Date { get; set; }
        public string Period { get; set; }
        public int AppointmentCount { get; set; }
        public int MaxAppointments { get; set; }
        public bool IsFullyBooked { get; set; }
        public bool IsRegistered { get; set; }
    }
}
