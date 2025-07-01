using MediAppointment.Domain.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Domain.Entities
{
    public class TimeSlot : Entity
    {
        public TimeSpan TimeStart { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan TimeEnd => TimeStart + Duration;
        public bool Shift { get; set; }
        public virtual ICollection<RoomTimeSlot> RoomSlots { get; set; } = new List<RoomTimeSlot>();
    }
}
