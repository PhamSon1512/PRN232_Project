using AutoMapper;
using MediAppointment.Application.DTOs.AppointmentDTOs;
using MediAppointment.Application.DTOs.TimeSlotDTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Enums;
using MediAppointment.Domain.Interfaces;
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
        private readonly IGenericRepository<Appointment> _appointmentRepository;

        public AppointmentService(ApplicationDbContext context,IMapper mapper, IGenericRepository<Appointment> appointmentRepository) {
            _context = context;
            _mapper = mapper;
            _appointmentRepository = appointmentRepository;
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
            var RoomTimeSlot = await _context.RoomTimeSlot
    .Include(x => x.Room)
    .Where(x =>
        x.Room.DepartmentId == request.DepartmentId &&
        x.TimeSlotId == request.TimeSlotId &&
        x.Date == request.Date &&
        x.Status != RoomTimeSlotStatus.Booked)
       .FirstOrDefaultAsync();
            if (RoomTimeSlot != null){
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

        // lấy các lịch hẹn của bác sĩ
        public async Task<IEnumerable<AppointmentResponse>> ListAppointmentsAssignedToDoctor(Guid doctorId, DateTime? date = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _appointmentRepository.Entities
                .Include(a => a.RoomTimeSlot)
                    .ThenInclude(rts => rts.Room)
                .Include(a => a.RoomTimeSlot)
                    .ThenInclude(rts => rts.TimeSlot)
                .Where(a => a.RoomTimeSlot.DoctorId == doctorId);

            if (date.HasValue)
                query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);
            else if (startDate.HasValue && endDate.HasValue)
                query = query.Where(a =>
                    a.AppointmentDate.Date >= startDate.Value.Date &&
                    a.AppointmentDate.Date <= endDate.Value.Date);

            var appointments = await query.ToListAsync();
            return _mapper.Map<List<AppointmentResponse>>(appointments);
        }



        public async Task<IEnumerable<TimeSlotExsitResponse>> GetTimeSlotExsit(GetTimeSlotExistDTO request)
        {
            var ListTimeSlot= await _context.TimeSlot.ToListAsync();
            List<TimeSlotDTO> ListTimeSlotDTO = new List<TimeSlotDTO>();
            foreach (var timeSlot in ListTimeSlot)
            {
              var temp=_mapper.Map<TimeSlotDTO>(timeSlot);
              ListTimeSlotDTO.Add(temp);
            }
            var ListRoomId = await _context.Room.Include(x => x.Department).Where(x => x.DepartmentId == request.DepartmentId).Select(x => x.Id).ToListAsync();
            List<TimeSlotExsitResponse> timeSlotExsitResponses = new List<TimeSlotExsitResponse>();
            for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
            {
                foreach (var timeslotdto in ListTimeSlotDTO)
                {
                    int counttimeslotbooked = await _context.RoomTimeSlot.Where(x => ListRoomId.Contains(x.RoomId) && x.Date == date && x.TimeSlotId == timeslotdto.Id && x.Status == RoomTimeSlotStatus.Booked).CountAsync();

                    TimeSlotExsitResponse timeSlotExsitResponse = new TimeSlotExsitResponse();
                    timeSlotExsitResponse.TimeSlot = timeslotdto;
                    timeSlotExsitResponse.DateTime = date;
                    if (counttimeslotbooked == ListRoomId.Count())
                    {
                        timeSlotExsitResponse.IsFull = true;
                    }
                    else
                    {
                        timeSlotExsitResponse.IsFull = false;
                    }
                    timeSlotExsitResponses.Add(timeSlotExsitResponse);

                }
            }
            return timeSlotExsitResponses;
        }
    }
}
