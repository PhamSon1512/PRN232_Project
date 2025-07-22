using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MediAppointment.Client.Models.Doctor;

namespace MediAppointment.Client.Models.Appointment
{
    public class BookingDoctorViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn khoa/phòng khám.")]
        public Guid? RoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bác sĩ.")]
        public Guid? DoctorId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ khám.")]
        public Guid? TimeSlotId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày khám.")]
        [DataType(DataType.Date)]
        public DateTime? AppointmentDate { get; set; }

        public string Note { get; set; }

        // Dropdown data
        public List<DepartmentOption> Departments { get; set; } = new();
        public List<DoctorViewModel> Doctors { get; set; } = new();
        public List<TimeSlotOption> TimeSlots { get; set; } = new();
    }
}
