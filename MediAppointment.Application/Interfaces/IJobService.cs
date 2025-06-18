using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.Interfaces
{
    public interface IJobService
    {
        Task JobCreateRoomTimeSlot();
    }
}
