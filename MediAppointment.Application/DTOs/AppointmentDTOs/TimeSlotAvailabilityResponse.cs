using MediAppointment.Application.DTOs.TimeSlotDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.AppointmentDTOs
{
    public class TimeSlotAvailabilityResponse
    {
        public DateTime Date { get; set; }
        public required TimeSlotDTO TimeSlot { get; set; }
        public bool IsAvailable { get; set; }
        public int AvailableRooms { get; set; }
        public int TotalRooms { get; set; }
        public List<RoomAvailability> RoomDetails { get; set; } = new List<RoomAvailability>();
    }

    public class RoomAvailability
    {
        public Guid RoomId { get; set; }
        public required string RoomName { get; set; }
        public bool IsAvailable { get; set; }
        public Guid? RoomTimeSlotId { get; set; }
    }
}
