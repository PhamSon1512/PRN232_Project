using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Domain.Entities
{
    public class RoomTimeSlot : Entity
    {
        public RoomTimeSlotStatus Status { get; set; }
        public Guid TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public Guid RoomId { get; set; }
        public Room Room { get; set; }
        public Guid? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public DateTime Date {  get; set; }
    }
}

