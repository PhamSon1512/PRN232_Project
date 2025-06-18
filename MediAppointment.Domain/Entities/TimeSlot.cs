using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Domain.Entities
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }
        public virtual ICollection<RoomTimeSlot> RoomSlots { get; set; } = new List<RoomTimeSlot>();
    }
}
