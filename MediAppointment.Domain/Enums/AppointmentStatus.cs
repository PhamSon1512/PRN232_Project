namespace MediAppointment.Domain.Enums
{
    public enum AppointmentStatus
    {
        Scheduled = 0,    // Đã đặt lịch
        Completed = 1,    // Hoàn thành
        Cancelled = 2,    // Đã hủy
        Rescheduled = 3,  // Đã dời lịch
        Pending = 4       // Chờ xác nhận
    }
}
