using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.AppointmentDTOs
{
    public class DepartmentResponse
    {
        public Guid Id { get; set; }
        public required string DepartmentName { get; set; }
        public int TotalRooms { get; set; }
    }
}
