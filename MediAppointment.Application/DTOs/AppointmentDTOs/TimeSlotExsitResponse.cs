using MediAppointment.Application.DTOs.TimeSlotDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.AppointmentDTOs
{
    public class TimeSlotExsitResponse
    {
        public DateTime DateTime { get; set; }
        public TimeSlotDTO TimeSlot { get; set; }
        public bool IsFull { get; set; }
    }
}
