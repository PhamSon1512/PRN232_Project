using MediAppointment.Application.DTOs.DoctorScheduleDTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Entities.Abstractions;
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
                        if (RoomTimeSlot.DoctorId != null)
                        {
                            throw new Exception("This Shift is Booked");
                        }
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

        public async Task<List<DoctorScheduleResponse>> GetDoctorSchedule(DoctorScheduleRequestDTO request)
        {
            List<DoctorScheduleResponse> list = new List<DoctorScheduleResponse>();
            Guid TimeSlotMoning = await _context.TimeSlot.Where(x => x.Shift).Select(x=>x.Id).FirstOrDefaultAsync();
            Guid TimeSlotMoon = await _context.TimeSlot.Where(x => !x.Shift).Select(x => x.Id).FirstOrDefaultAsync();
            int x = 0;
            if(request.DoctorId== null)
            {
                
                DateTime currentDate = request.DateStart.Date; 
                DateTime endDate = request.DateEnd.Date;
                while (currentDate <= endDate)
                {                  
                    DoctorScheduleResponse response = new DoctorScheduleResponse();
                    response.Date=currentDate;
                    var query = _context.RoomTimeSlot.Where(x => x.RoomId == request.RoomId&& x.Date==currentDate).AsQueryable();
                    var DoctorIdMoning= await query.Where(x => x.TimeSlotId == TimeSlotMoning).Select(x=>x.DoctorId).FirstOrDefaultAsync();
                    var DoctorIdMoon = await query.Where(x => x.TimeSlotId == TimeSlotMoon).Select(x => x.DoctorId).FirstOrDefaultAsync();
                    var Doctor1= await  _context.Set<User>().OfType<Doctor>().FirstOrDefaultAsync(x=>x.Id==DoctorIdMoning);
                    var NameDoctor1 = Doctor1?.FullName;
                    var Doctor2 = await _context.Set<User>().OfType<Doctor>().FirstOrDefaultAsync(x => x.Id == DoctorIdMoon);
                    var NameDoctor2 = Doctor2?.FullName;
                    response.DoctorNameMorning = NameDoctor1;
                    response.DoctorNameAfternoon = NameDoctor2;
                    list.Add(response);
                    currentDate = currentDate.AddDays(1);
                }
            }
            else
            {
                DateTime currentDate = request.DateStart.Date;
                DateTime endDate = request.DateEnd.Date;

                while (currentDate <= endDate)
                {
                    DoctorScheduleResponse response = new DoctorScheduleResponse();
                    response.Date=currentDate;
                    
                    var Monring = _context.RoomTimeSlot.Include(x=>x.Room).Where(x => x.DoctorId == request.DoctorId && x.Date == currentDate&&x.TimeSlotId==TimeSlotMoning).Select(x=>x.Room.Name).FirstOrDefault();
                    var Moon = _context.RoomTimeSlot.Include(x => x.Room).Where(x => x.DoctorId == request.DoctorId && x.Date == currentDate && x.TimeSlotId == TimeSlotMoon).Select(x => x.Room.Name).FirstOrDefault();
                    response.RoomMorning = Monring;
                    response.RoomAfternoon = Moon;
                    list.Add(response);
                    currentDate = currentDate.AddDays(1);
                }
            }
            return list;
        }
    }
}
