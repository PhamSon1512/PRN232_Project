using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.RoomTimeSlotDTOs
{
    public class RoomTimeSlotResponse
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string RoomName { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }
        public string Status { get; set; }
    }
}
