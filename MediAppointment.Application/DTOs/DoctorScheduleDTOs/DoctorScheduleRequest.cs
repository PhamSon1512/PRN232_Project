using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.DoctorScheduleDTOs
{
    public class DoctorScheduleRequest
    {
        public Guid RoomId { get; set; }
        public bool Shift { get; set; }
        public DateTime DateSchedule { get; set; }
    }
}
