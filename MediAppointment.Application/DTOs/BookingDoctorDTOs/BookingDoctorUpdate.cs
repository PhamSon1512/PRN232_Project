using System;
using System.ComponentModel.DataAnnotations;

namespace MediAppointment.Application.DTOs.BookingDoctorDTOs
{
    public class BookingDoctorUpdate
    {
        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid TimeSlotId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public string? Note { get; set; }

        [Required]
        [RegularExpression("Pending|Confirmed|Cancelled|Completed", ErrorMessage = "Trạng thái không hợp lệ.")]
        public string Status { get; set; }
    }
}
