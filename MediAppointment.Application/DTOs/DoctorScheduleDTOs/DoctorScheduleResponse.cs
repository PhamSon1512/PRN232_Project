using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.DoctorScheduleDTOs
{
    public class DoctorScheduleResponse
    {
        public DateTime Date { get; set; }
        public string? DoctorNameMorning { get; set; }
        public string? DoctorNameAfternoon { get; set; }
        public string? RoomMorning { get; set; }
        public string? RoomAfternoon { get; set; }

    }
}
