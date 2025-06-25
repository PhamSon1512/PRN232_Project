using MediAppointment.Application.DTOs.DoctorScheduleDTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
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
                var ListTimeSlot = _context.TimeSlot.Where(x => x.Shift == request.Shift).ToList();
                foreach (var xtimeslot in ListTimeSlot) {
                    var RoomTimeSlot = await _context.RoomTimeSlot.Where(x => x.RoomId == request.RoomId&&x.TimeSlotId==xtimeslot.Id && x.Date == request.DateSchedule && x.Status == Domain.Enums.RoomTimeSlotStatus.Available).FirstOrDefaultAsync();
                    if (RoomTimeSlot != null)
                    {
                        RoomTimeSlot.DoctorId = DoctorId;
                    }
                    else
                    {
                        throw new Exception("SomeThing is Wrong");
                    }
                }
                
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDoctorSchedule(Guid DoctorId,DeleteDoctorScheduleDTO request)
        {
            var ListTimeSlot=await _context.TimeSlot.Where(x=>x.Shift== request.Shift).ToListAsync(); 
            foreach(var xtimeslot in ListTimeSlot)
            {
                var RoomTimeSlot = await _context.RoomTimeSlot.Where(x => x.TimeSlotId == xtimeslot.Id &&x.Date==request.date&&x.DoctorId==DoctorId&&x.RoomId==request.RoomId).FirstOrDefaultAsync();
                if (RoomTimeSlot != null)
                {
                    RoomTimeSlot.DoctorId = null;
                }
                else
                {
                    throw new Exception("Something is wrong");
                }

            }
            await _context.SaveChangesAsync();
        }
    }
}
