using MediAppointment.Domain.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Domain.Entities
{
    public class Room : Entity
    {

        public string Name { get; set; }
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; }
        public virtual ICollection<RoomTimeSlot> RoomTimeSlots { get; set; }
    }
}
