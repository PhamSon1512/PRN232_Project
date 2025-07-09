using System;

namespace MediAppointment.Application.DTOs.DoctorScheduleDTOs
{
    public class DoctorScheduleDeleteRequest
    {
        public DateTime Date { get; set; }
        public string Period { get; set; } = string.Empty; // "morning" or "afternoon"
        public Guid RoomId { get; set; }
    }
}
