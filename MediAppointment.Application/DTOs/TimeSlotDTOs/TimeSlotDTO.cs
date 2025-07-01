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
    }
}
