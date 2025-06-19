using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.AppointmentDTOs
{
    public class AppointmentResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; } 
        public string RoomName { get; set; }
        public string Time {  get; set; }
    }
}
