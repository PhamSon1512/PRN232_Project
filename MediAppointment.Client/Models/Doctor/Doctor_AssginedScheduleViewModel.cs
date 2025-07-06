namespace MediAppointment.Client.Models.Doctor
{
    public class Doctor_AssginedScheduleViewModel
    {
        public int SelectedYear { get; set; }
        public string SelectedWeek { get; set; } = string.Empty;
        public List<int> AvailableYears { get; set; } = new();
        public List<string> AvailableWeeks { get; set; } = new();

        public Dictionary<DateTime, List<ScheduleSlot>> MorningSlots { get; set; } = new();
        public Dictionary<DateTime, List<ScheduleSlot>> AfternoonSlots { get; set; } = new();
    }

    public class ScheduleSlot
    {
        public string RoomName { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public DateTime Date { get; set; }

    }
}
