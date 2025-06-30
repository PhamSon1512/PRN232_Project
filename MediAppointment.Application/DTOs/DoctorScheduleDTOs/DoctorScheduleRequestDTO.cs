using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.DoctorScheduleDTOs
{
    public class DoctorScheduleRequestDTO
    {
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public Guid RoomId { get; set; }
        public Guid? DoctorId { get; set; }
    }
}
