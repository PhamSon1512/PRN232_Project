using MediAppointment.Application.DTOs.AppointmentDTOs;
using MediAppointment.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.RoomTimeSlotDTOs
{
    public class RoomTimeSlotDetailResponse
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public RoomTimeSlotStatus Status { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string TimeStart { get; set; } = string.Empty;
        public string TimeEnd { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Shift {  get; set; } = string.Empty;
        public List<AppointmentResponse> Appointments { get; set; } = new();
    }
}
