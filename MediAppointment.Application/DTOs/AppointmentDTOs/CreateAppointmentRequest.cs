using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.AppointmentDTOs
{
    public class CreateAppointmentRequest
    {
        public Guid TimeSlotId { get; set; }
        public DateTime Date { get; set; }
        public Guid DepartmentId { get; set; }

    }
}
