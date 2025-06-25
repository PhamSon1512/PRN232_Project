using MediAppointment.Application.DTOs.DoctorScheduleDTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Infrastructure.Services
{
    public class DoctorScheduleService : IDoctorScheduleService
    {
        protected readonly ApplicationDbContext _context;
        public DoctorScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task CreateDoctorSchedule(Guid DoctorId,List<DoctorScheduleRequest> requests) {
            foreach (var request in requests) {
                var RoomTimeSlot = await _context.RoomTimeSlot.Where(x => x.RoomId == request.RoomId && x.TimeSlotId == request.TimeSlotId && x.Date == request.DateSchedule && x.Status == Domain.Enums.RoomTimeSlotStatus.Available).FirstOrDefaultAsync();
                if (RoomTimeSlot != null) {
                    RoomTimeSlot.DoctorId = DoctorId;
                }
                else
                {
                    throw new Exception("SomeThing is Wrong");
                }
                
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDoctorSchedule(Guid RoomTimeSlotId)
        {
            var RoomTimeSlot = await _context.RoomTimeSlot.FirstOrDefaultAsync(x=>x.Id == RoomTimeSlotId);
            RoomTimeSlot.DoctorId = null;
            _context.RoomTimeSlot.Update(RoomTimeSlot);
            await _context.SaveChangesAsync();
        }
    }
}
