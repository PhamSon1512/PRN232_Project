using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.AppointmentDTOs
{
    public class BookAppointmentRequest
    {
        public Guid DepartmentId { get; set; }
        public Guid TimeSlotId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Note { get; set; }
    }
}
