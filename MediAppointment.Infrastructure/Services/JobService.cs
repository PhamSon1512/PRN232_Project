using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Enums;
using MediAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Infrastructure.Services
{
    public class JobService : IJobService
    {
        private readonly ApplicationDbContext _context;

        public JobService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task JobCreateRoomTimeSlot()
        {
            var targetDate = DateTime.Today.AddDays(4);
            var rooms = await _context.Room.ToListAsync();
            var timeSlots = await _context.TimeSlot.ToListAsync();

            foreach (var room in rooms)
            {
                foreach (var timeSlot in timeSlots)
                {
                    var exists = await _context.RoomTimeSlot.AnyAsync(rts =>
                        rts.RoomId == room.Id &&
                        rts.TimeSlotId == timeSlot.Id &&
                        rts.Date == targetDate);

                    if (!exists)
                    {
                        _context.RoomTimeSlot.Add(new RoomTimeSlot
                        {
                            Id = Guid.NewGuid(),
                            TimeSlotId = timeSlot.Id,
                            Date = targetDate,
                            RoomId = room.Id,
                            Status = RoomTimeSlotStatus.Available // Đảm bảo tạo với status Available
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
