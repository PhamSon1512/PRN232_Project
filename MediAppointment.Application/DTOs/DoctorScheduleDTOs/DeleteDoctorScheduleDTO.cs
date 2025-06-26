using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.DoctorScheduleDTOs
{
    public class DeleteDoctorScheduleDTO
    {
        public DateTime date {  get; set; }
        public bool Shift { get; set; }
        public Guid RoomId { get; set; }
    }
}
