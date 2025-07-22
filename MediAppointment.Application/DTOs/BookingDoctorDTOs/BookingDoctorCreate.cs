using System;
using System.ComponentModel.DataAnnotations;

namespace MediAppointment.Application.DTOs.BookingDoctorDTOs
{
    public class BookingDoctorCreate
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid TimeSlotId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public string? Note { get; set; }
    }

}
