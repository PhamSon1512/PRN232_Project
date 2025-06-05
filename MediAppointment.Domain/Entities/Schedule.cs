namespace MediAppointment.Domain.Entities
{
    using MediAppointment.Domain.Enums;
    public class Schedule
    {
        public int ScheduleID { get; set; }
        public int DoctorID { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Shift MorningShift { get; set; }
        public Shift AfternoonShift { get; set; }
        public Shift EveningShift { get; set; }
        public Doctor? Doctor { get; set; }
    }
}