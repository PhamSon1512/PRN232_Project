using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.TimeSlotDTOs
{
    public class TimeSlotDTO
    {
        public Guid Id { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan TimeEnd => TimeStart + Duration;
        public bool Shift { get; set; }
        public string TimeRange => $"{TimeStart:hh\\:mm} - {TimeEnd:hh\\:mm}";
    }
}
