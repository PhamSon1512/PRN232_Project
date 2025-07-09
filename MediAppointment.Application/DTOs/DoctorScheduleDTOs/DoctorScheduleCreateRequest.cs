using System;
using System.Collections.Generic;

namespace MediAppointment.Application.DTOs.DoctorScheduleDTOs
{
    public class DoctorScheduleCreateRequest
    {
        public Guid RoomId { get; set; }
        public DateTime Date { get; set; }
        public string Period { get; set; } = string.Empty; // "morning" or "afternoon"
        public List<Guid> TimeSlotIds { get; set; } = new List<Guid>();
    }
}
