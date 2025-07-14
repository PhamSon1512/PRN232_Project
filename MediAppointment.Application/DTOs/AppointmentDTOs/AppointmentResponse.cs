using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.AppointmentDTOs
{
    public class AppointmentResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid RoomTimeSlotId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        // Removed DoctorName and Time fields as they are no longer needed
    }
}
