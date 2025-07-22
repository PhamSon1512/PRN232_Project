namespace MediAppointment.Client.Models
{
    public class ConfirmationBookingViewModel
    {
        public string Department { get; set; }             // Khoa khám
        public DateTime AppointmentDate { get; set; }      // Ngày khám
        public string TimeRange { get; set; }              // Giờ khám (ví dụ: "08:00 - 09:00")
        public string? Note { get; set; }                  // Ghi chú
        public decimal DepositAmount { get; set; }         // Tiền cọc
    }
}
