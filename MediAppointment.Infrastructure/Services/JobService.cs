using MediAppointment.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Infrastructure.Services
{
    public class JobService : IJobService
    {
        public Task JobCreateRoomTimeSlot()
        {
            Console.WriteLine("NH");
            return Task.CompletedTask;
        }
    }
}
