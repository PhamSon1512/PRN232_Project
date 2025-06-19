using AutoMapper;
using MediAppointment.Application.DTOs.AppointmentDTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Enums;
using MediAppointment.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MediAppointment.Infrastructure.Services
{
    public class AppointmentService : IAppointmentService
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IMapper _mapper;
        public AppointmentService(ApplicationDbContext context,IMapper mapper) {
        _context = context;
        _mapper = mapper;
        }

        public async Task<IEnumerable<AppointmentResponse>> ListAppointmentByUser(Guid UserId)
        {
            var ListAppointment =await _context.Appointments.Where(x=>x.PatientId == UserId).Include(x => x.RoomTimeSlot)
            .ThenInclude(x => x.Room)
        .Include(x => x.RoomTimeSlot)
            .ThenInclude(x => x.TimeSlot).ToListAsync();
            List<AppointmentResponse> appointmentResponses = new List<AppointmentResponse>();
            foreach (var item in ListAppointment) {
              appointmentResponses.Add(_mapper.Map<AppointmentResponse>(item));
            }
            return appointmentResponses;
        }

        public async Task CreateAppointment(Guid UserId,CreateAppointmentRequest request)
        {
            var RoomTimeSlot =await _context.RoomTimeSlot.Include(x => x.Room)
                .Where(x => x.Room.DepartmentId == request.DepartmentId).
                 FirstOrDefaultAsync(x=>x.TimeSlotId==request.TimeSlotId&&x.Date==request.Date &&x.Status!=RoomTimeSlotStatus.Booked);
            if(RoomTimeSlot != null){
                RoomTimeSlot.Status = RoomTimeSlotStatus.Booked;
                _context.RoomTimeSlot.Update(RoomTimeSlot);
            }
            else
            {
                throw new Exception("Can not Find RoomTimeSlot");
            }
            Appointment appointment =new Appointment{
            Id=Guid.NewGuid(),
            PatientId=UserId,
            AppointmentDate=request.Date,
            CreatedDate=DateTime.Now,
            RoomTimeSlotId=RoomTimeSlot.Id,
            Status=AppointmentStatus.Scheduled,
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
        }
        public async Task<AppointmentResponse> AppointmentDetailById(Guid AppointmentId)
        {
            var Appointment = await _context.Appointments.Include(x => x.RoomTimeSlot)
            .ThenInclude(x => x.Room)
        .Include(x => x.RoomTimeSlot)
            .ThenInclude(x => x.TimeSlot).FirstOrDefaultAsync(x => x.Id == AppointmentId);
            var AppointmentDto = _mapper.Map<AppointmentResponse>(Appointment);
            return AppointmentDto;
        }

        public async Task CancelById(Guid AppointmentId)
        {
            var Appointment = await _context.Appointments.Include(x=>x.RoomTimeSlot).FirstOrDefaultAsync(x=>x.Id == AppointmentId);
            if(Appointment != null)
            {
                Appointment.Status =AppointmentStatus.Cancelled;
                Guid RoomTimeSlotId = Appointment.RoomTimeSlot.Id;
                _context.Appointments.Update(Appointment);
                var RoomTimeSlot= await _context.RoomTimeSlot.FirstOrDefaultAsync(x=>x.Id==RoomTimeSlotId);
                if (RoomTimeSlot != null) {
                    RoomTimeSlot.Status = RoomTimeSlotStatus.Available;
                    _context.RoomTimeSlot.Update(RoomTimeSlot);
                }
            }
            else
            {
                throw new Exception("Not Found AppointmentId");
            }
            await _context.SaveChangesAsync();
        }
    }
}
