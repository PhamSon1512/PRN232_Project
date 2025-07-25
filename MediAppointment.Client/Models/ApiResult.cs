namespace MediAppointment.Client.Models
{
    public class ApiResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public string? Details { get; set; } // <- thêm dòng này
    }
}
