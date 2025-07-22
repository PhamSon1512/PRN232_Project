using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.BookingDoctorDTOs
{
    public class BookingPatientResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid TimeSlotId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Optional: Thông tin mở rộng cho bệnh nhân
        public string? DoctorName { get; set; }
        public string? DepartmentName { get; set; }
        public string? TimeSlotDescription { get; set; }  // ví dụ: "08:00 - 09:00"
        public string? RoomName { get; set; }
    }
}
