using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.BookingDoctorDTOs
{
    public class BookingDoctorStatusUpdate
    {
        public string Status { get; set; } = string.Empty; // "Approved" hoặc "Rejected"
        public string? Note { get; set; } // Ghi chú, ví dụ lý do từ chối
    }
}
