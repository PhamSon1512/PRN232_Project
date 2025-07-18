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
            foreach (var request in requests)
            {
                // Yêu cầu request phải có TimeSlotId
                if (request.TimeSlotId == Guid.Empty)
                    throw new Exception("Thiếu TimeSlotId khi đăng ký ca làm việc!");

                var roomTimeSlot = await _context.RoomTimeSlot
                    .FirstOrDefaultAsync(x => x.RoomId == request.RoomId && x.TimeSlotId == request.TimeSlotId && x.Date == request.DateSchedule);

                if (roomTimeSlot != null)
                {
                    if (roomTimeSlot.DoctorId != null && roomTimeSlot.DoctorId != DoctorId)
                    {
                        throw new Exception("This Shift is already booked by another doctor");
                    }
                    roomTimeSlot.DoctorId = DoctorId;
                    roomTimeSlot.Status = Domain.Enums.RoomTimeSlotStatus.Booked;
                }
                else
                {
                    var newRoomTimeSlot = new RoomTimeSlot
                    {
                        Id = Guid.NewGuid(),
                        RoomId = request.RoomId,
                        TimeSlotId = request.TimeSlotId,
                        Date = request.DateSchedule,
                        DoctorId = DoctorId,
                        Status = Domain.Enums.RoomTimeSlotStatus.Booked
                    };
                    _context.RoomTimeSlot.Add(newRoomTimeSlot);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDoctorSchedule(Guid DoctorId,DeleteDoctorScheduleDTO request)
        {
            try
            {
                var ListTimeSlot = await _context.TimeSlot.Where(x => x.Shift == request.Shift).ToListAsync();
                
                if (!ListTimeSlot.Any())
                {
                    throw new Exception($"Không tìm thấy khung giờ cho ca {(request.Shift ? "chiều" : "sáng")}");
                }
                
                int deletedCount = 0;
                foreach(var xtimeslot in ListTimeSlot)
                {
                    var RoomTimeSlot = await _context.RoomTimeSlot
                        .Where(x => x.TimeSlotId == xtimeslot.Id && 
                                   x.Date.Date == request.date.Date && 
                                   x.DoctorId == DoctorId && 
                                   x.RoomId == request.RoomId)
                        .FirstOrDefaultAsync();
                        
                    if (RoomTimeSlot != null)
                    {
                        RoomTimeSlot.DoctorId = null;
                        RoomTimeSlot.Status = Domain.Enums.RoomTimeSlotStatus.Available;
                        deletedCount++;
                    }
                }
                
                if (deletedCount == 0)
                {
                    throw new Exception($"Không tìm thấy lịch làm việc ca {(request.Shift ? "chiều" : "sáng")} ngày {request.date:dd/MM/yyyy} để hủy");
                }
                
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Re-throw the exception to be handled by the controller
                throw new Exception($"Lỗi khi hủy lịch làm việc: {ex.Message}", ex);
            }
        }

        public async Task<List<DoctorScheduleResponse>> GetDoctorSchedule(DoctorScheduleRequestDTO request)
        {
            List<DoctorScheduleResponse> list = new List<DoctorScheduleResponse>();
            
            if(request.DoctorId== null)
            {
                DateTime currentDate = request.DateStart.Date; 
                DateTime endDate = request.DateEnd.Date;
                while (currentDate <= endDate)
                {                  
                    DoctorScheduleResponse response = new DoctorScheduleResponse();
                    response.Date=currentDate;
                    var query = _context.RoomTimeSlot.Where(x => x.RoomId == request.RoomId&& x.Date==currentDate).AsQueryable();
                    
                    // Check if any morning time slots are booked by any doctor
                    var morningTimeSlots = await _context.TimeSlot.Where(x => !x.Shift).Select(x => x.Id).ToListAsync();
                    var DoctorIdMorning = await query.Where(x => morningTimeSlots.Contains(x.TimeSlotId) && x.DoctorId != null).Select(x=>x.DoctorId).FirstOrDefaultAsync();
                    
                    // Check if any afternoon time slots are booked by any doctor
                    var afternoonTimeSlots = await _context.TimeSlot.Where(x => x.Shift).Select(x => x.Id).ToListAsync();
                    var DoctorIdAfternoon = await query.Where(x => afternoonTimeSlots.Contains(x.TimeSlotId) && x.DoctorId != null).Select(x=>x.DoctorId).FirstOrDefaultAsync();
                    
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
                    
                    // Check if doctor has ANY morning time slots for this date
                    var morningTimeSlots = await _context.TimeSlot.Where(x => !x.Shift).Select(x => x.Id).ToListAsync();
                    var Morning = await _context.RoomTimeSlot.Include(x=>x.Room)
                        .Where(x => x.DoctorId == request.DoctorId && x.Date == currentDate && morningTimeSlots.Contains(x.TimeSlotId))
                        .Select(x=>x.Room.Name).FirstOrDefaultAsync();
                    
                    // Check if doctor has ANY afternoon time slots for this date
                    var afternoonTimeSlots = await _context.TimeSlot.Where(x => x.Shift).Select(x => x.Id).ToListAsync();
                    var Afternoon = await _context.RoomTimeSlot.Include(x => x.Room)
                        .Where(x => x.DoctorId == request.DoctorId && x.Date == currentDate && afternoonTimeSlots.Contains(x.TimeSlotId))
                        .Select(x => x.Room.Name).FirstOrDefaultAsync();
                        
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
