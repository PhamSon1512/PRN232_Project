using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.AppointmentDTOs
{
    public class GetTimeSlotExistDTO
    {
        public Guid DepartmentId { get; set; }
        public DateTime StartDate {  get; set; }
        public DateTime EndDate { get; set; }
    }
}
