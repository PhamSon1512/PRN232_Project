using System;

namespace MediAppointment.Application.DTOs.BookingDoctorDTOs
{
    public class BookingDoctorResponse
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

        // Optional: thông tin mở rộng (nếu bạn muốn gắn thêm thông tin bác sĩ/khoa/tên bệnh nhân)
        public string? DoctorName { get; set; }
        public string? DepartmentName { get; set; }
        public string? PatientName { get; set; }
    }
}
