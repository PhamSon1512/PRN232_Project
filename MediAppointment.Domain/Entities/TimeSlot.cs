using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Domain.Entities
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan TimeEnd => TimeStart + Duration;
        public virtual ICollection<RoomTimeSlot> RoomSlots { get; set; } = new List<RoomTimeSlot>();
    }
}
