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
                    var RoomTimeSlot = await _context.RoomTimeSlot.Where(x => x.RoomId == request.RoomId&&x.TimeSlotId==xtimeslot.Id && x.Date == request.DateSchedule).FirstOrDefaultAsync();
                    
                    if (RoomTimeSlot != null)
                    {
                        // Check if already booked by another doctor
                        if (RoomTimeSlot.DoctorId != null && RoomTimeSlot.DoctorId != DoctorId)
                        {
                            throw new Exception("This Shift is already booked by another doctor");
                        }
                        // Update doctor assignment
                        RoomTimeSlot.DoctorId = DoctorId;
                        RoomTimeSlot.Status = Domain.Enums.RoomTimeSlotStatus.Booked;
                    }
                    else
                    {
                        // Create new RoomTimeSlot if doesn't exist
                        var newRoomTimeSlot = new RoomTimeSlot
                        {
                            Id = Guid.NewGuid(),
                            RoomId = request.RoomId,
                            TimeSlotId = xtimeslot.Id,
                            Date = request.DateSchedule,
                            DoctorId = DoctorId,
                            Status = Domain.Enums.RoomTimeSlotStatus.Booked
                        };
                        _context.RoomTimeSlot.Add(newRoomTimeSlot);
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
                    RoomTimeSlot.Status = Domain.Enums.RoomTimeSlotStatus.Available;
                }
                // If RoomTimeSlot doesn't exist, it means doctor wasn't scheduled for this slot, so we can ignore it

            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<DoctorScheduleResponse>> GetDoctorSchedule(DoctorScheduleRequestDTO request)
        {
            List<DoctorScheduleResponse> list = new List<DoctorScheduleResponse>();
            Guid TimeSlotMorning = await _context.TimeSlot.Where(x => !x.Shift).Select(x=>x.Id).FirstOrDefaultAsync();
            Guid TimeSlotAfternoon = await _context.TimeSlot.Where(x => x.Shift).Select(x => x.Id).FirstOrDefaultAsync();
            
            if(request.DoctorId== null)
            {
                
                DateTime currentDate = request.DateStart.Date; 
                DateTime endDate = request.DateEnd.Date;
                while (currentDate <= endDate)
                {                  
                    DoctorScheduleResponse response = new DoctorScheduleResponse();
                    response.Date=currentDate;
                    var query = _context.RoomTimeSlot.Where(x => x.RoomId == request.RoomId&& x.Date==currentDate).AsQueryable();
                    var DoctorIdMorning= await query.Where(x => x.TimeSlotId == TimeSlotMorning).Select(x=>x.DoctorId).FirstOrDefaultAsync();
                    var DoctorIdAfternoon = await query.Where(x => x.TimeSlotId == TimeSlotAfternoon).Select(x => x.DoctorId).FirstOrDefaultAsync();
                    var Doctor1= await  _context.Set<User>().OfType<Doctor>().FirstOrDefaultAsync(x=>x.Id==DoctorIdMorning);
                    var NameDoctor1 = Doctor1?.FullName;
                    var Doctor2 = await _context.Set<User>().OfType<Doctor>().FirstOrDefaultAsync(x => x.Id == DoctorIdAfternoon);
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
                    
                    var Morning = _context.RoomTimeSlot.Include(x=>x.Room).Where(x => x.DoctorId == request.DoctorId && x.Date == currentDate&&x.TimeSlotId==TimeSlotMorning).Select(x=>x.Room.Name).FirstOrDefault();
                    var Afternoon = _context.RoomTimeSlot.Include(x => x.Room).Where(x => x.DoctorId == request.DoctorId && x.Date == currentDate && x.TimeSlotId == TimeSlotAfternoon).Select(x => x.Room.Name).FirstOrDefault();
                    response.RoomMorning = Morning;
                    response.RoomAfternoon = Afternoon;
                    list.Add(response);
                    currentDate = currentDate.AddDays(1);
                }
            }
            return list;
        }
    }
}
