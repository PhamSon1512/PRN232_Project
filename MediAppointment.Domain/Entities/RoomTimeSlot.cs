using MediAppointment.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Domain.Entities
{
    public class RoomTimeSlot
    {
        public int Id { get; set; }
        public RoomTimeSlotStatus Status { get; set; }
        public int TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }
        public Guid? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
    }
}

