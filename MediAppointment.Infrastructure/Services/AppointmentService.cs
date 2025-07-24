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
            var ListAppointment = await _context.Appointments
                .Where(x => x.PatientId == UserId)
                .Include(x => x.RoomTimeSlot)
                    .ThenInclude(x => x.Room)
                        .ThenInclude(x => x.Department) // Include Department
                .Include(x => x.RoomTimeSlot)
                    .ThenInclude(x => x.TimeSlot)
                .ToListAsync();
                
            List<AppointmentResponse> appointmentResponses = new List<AppointmentResponse>();
            foreach (var item in ListAppointment) 
            {
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
        x.Date.Date == request.Date.Date && // So sánh chính xác Date
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
            var Appointment = await _context.Appointments
                .Include(x => x.RoomTimeSlot)
                    .ThenInclude(x => x.Room)
                        .ThenInclude(x => x.Department) // Include Department
                .Include(x => x.RoomTimeSlot)
                    .ThenInclude(x => x.TimeSlot)
                .FirstOrDefaultAsync(x => x.Id == AppointmentId);
                
            var AppointmentDto = _mapper.Map<AppointmentResponse>(Appointment);
            return AppointmentDto;
        }

        public async Task CancelById(Guid AppointmentId)
        {
            var Appointment = await _context.Appointments.Include(x=>x.RoomTimeSlot).FirstOrDefaultAsync(x=>x.Id == AppointmentId);
            if(Appointment != null)
            {
                Appointment.Status = AppointmentStatus.Cancelled;
                Guid RoomTimeSlotId = Appointment.RoomTimeSlot.Id;
                _context.Appointments.Update(Appointment);
                
                var RoomTimeSlot = await _context.RoomTimeSlot.FirstOrDefaultAsync(x=>x.Id==RoomTimeSlotId);
                if (RoomTimeSlot != null) {
                    RoomTimeSlot.Status = RoomTimeSlotStatus.Available; // Reset về Available khi cancel
                    _context.RoomTimeSlot.Update(RoomTimeSlot);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Not Found AppointmentId");
            }
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

        public async Task<IEnumerable<DepartmentResponse>> GetDepartments()
        {
            var departments = await _context.Departments
                .Include(d => d.Rooms)
                .ToListAsync();

            return departments.Select(d => new DepartmentResponse
            {
                Id = d.Id,
                DepartmentName = d.DepartmentName ?? "",
                TotalRooms = d.Rooms?.Count ?? 0
            });
        }

        public async Task<IEnumerable<TimeSlotAvailabilityResponse>> GetAvailableTimeSlotsForBooking(GetTimeSlotExistDTO request, Guid? userId = null)
        {
            var timeSlots = await _context.TimeSlot
                .OrderBy(ts => ts.Shift) // Sáng trước, chiều sau
                .ThenBy(ts => ts.TimeStart) // Trong cùng ca, sắp xếp theo giờ bắt đầu
                .ToListAsync();
            var departmentRooms = await _context.Room
                .Where(r => r.DepartmentId == request.DepartmentId)
                .OrderBy(r => r.Name) // Sắp xếp phòng theo tên
                .ToListAsync();

            var result = new List<TimeSlotAvailabilityResponse>();

            for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
            {
                foreach (var timeSlot in timeSlots)
                {
                    var roomAvailabilities = new List<RoomAvailability>();
                    int availableRoomsCount = 0;

                    // Kiểm tra xem bệnh nhân đã đặt lịch trong cùng khoa và cùng thời gian hay chưa
                    bool patientAlreadyBooked = false;
                    if (userId.HasValue)
                    {
                        patientAlreadyBooked = await _context.Appointments
                            .Include(a => a.RoomTimeSlot)
                                .ThenInclude(rts => rts.Room)
                            .AnyAsync(a => 
                                a.PatientId == userId.Value &&
                                a.RoomTimeSlot.Room.DepartmentId == request.DepartmentId &&
                                a.RoomTimeSlot.TimeSlotId == timeSlot.Id &&
                                a.AppointmentDate.Date == date.Date &&
                                a.Status == AppointmentStatus.Scheduled);
                    }

                    foreach (var room in departmentRooms)
                    {
                        var roomTimeSlot = await _context.RoomTimeSlot
                            .FirstOrDefaultAsync(rts => 
                                rts.RoomId == room.Id &&
                                rts.TimeSlotId == timeSlot.Id &&
                                rts.Date.Date == date.Date);

                        // Cải thiện logic kiểm tra availability
                        bool isRoomAvailable;
                        if (roomTimeSlot == null)
                        {
                            // Nếu chưa có RoomTimeSlot, coi như available
                            isRoomAvailable = true;
                        }
                        else
                        {
                            // Nếu đã có RoomTimeSlot, kiểm tra status chính xác
                            isRoomAvailable = roomTimeSlot.Status == RoomTimeSlotStatus.Available;
                        }
                        
                        if (isRoomAvailable)
                        {
                            availableRoomsCount++;
                        }

                        roomAvailabilities.Add(new RoomAvailability
                        {
                            RoomId = room.Id,
                            RoomName = room.Name ?? "Unknown Room",
                            IsAvailable = isRoomAvailable,
                            RoomTimeSlotId = roomTimeSlot?.Id
                        });
                    }

                    var mappedTimeSlot = _mapper.Map<TimeSlotDTO>(timeSlot);
                    
                    // Nếu bệnh nhân đã đặt lịch trong cùng khoa và cùng thời gian, đánh dấu là không available
                    bool isTimeSlotAvailable = availableRoomsCount > 0 && !patientAlreadyBooked;
                    
                    result.Add(new TimeSlotAvailabilityResponse
                    {
                        Date = date,
                        TimeSlot = mappedTimeSlot,
                        IsAvailable = isTimeSlotAvailable,
                        AvailableRooms = patientAlreadyBooked ? 0 : availableRoomsCount, // Hiển thị 0 nếu đã đặt
                        TotalRooms = departmentRooms.Count,
                        RoomDetails = roomAvailabilities
                    });
                }
            }

            // Sắp xếp kết quả cuối cùng theo shift và thời gian
            return result.OrderBy(r => r.TimeSlot.Shift)
                        .ThenBy(r => r.TimeSlot.TimeStart);
        }

        public async Task BookAppointment(Guid userId, BookAppointmentRequest request)
        {
            // Tìm tất cả các phòng trong department
            var departmentRooms = await _context.Room
                .Where(r => r.DepartmentId == request.DepartmentId)
                .ToListAsync();

            if (!departmentRooms.Any())
            {
                throw new Exception("Không tìm thấy phòng khám cho khoa này");
            }

            // userId từ JWT claim chính là Patient.Id
            var patient = await _context.Patients.FindAsync(userId);
            if (patient == null)
            {
                throw new Exception($"Không tìm thấy thông tin bệnh nhân với ID: {userId}. Chỉ bệnh nhân mới có thể đặt lịch khám.");
            }

            // Tìm RoomTimeSlot available đầu tiên
            RoomTimeSlot? availableRoomTimeSlot = null;

            foreach (var room in departmentRooms)
            {
                var existingRoomTimeSlot = await _context.RoomTimeSlot
                    .FirstOrDefaultAsync(rts =>
                        rts.RoomId == room.Id &&
                        rts.TimeSlotId == request.TimeSlotId &&
                        rts.Date.Date == request.AppointmentDate.Date);

                if (existingRoomTimeSlot != null && existingRoomTimeSlot.Status == RoomTimeSlotStatus.Available)
                {
                    // Sử dụng RoomTimeSlot có sẵn với status Available
                    availableRoomTimeSlot = existingRoomTimeSlot;
                    break;
                }
            }

            // Nếu không tìm thấy RoomTimeSlot available, tạo mới
            if (availableRoomTimeSlot == null)
            {
                // Tìm room đầu tiên chưa có RoomTimeSlot cho time slot này
                var roomWithoutTimeSlot = departmentRooms.FirstOrDefault(room => 
                    !_context.RoomTimeSlot.Any(rts =>
                        rts.RoomId == room.Id &&
                        rts.TimeSlotId == request.TimeSlotId &&
                        rts.Date.Date == request.AppointmentDate.Date));

                if (roomWithoutTimeSlot != null)
                {
                    availableRoomTimeSlot = new RoomTimeSlot
                    {
                        Id = Guid.NewGuid(),
                        RoomId = roomWithoutTimeSlot.Id,
                        TimeSlotId = request.TimeSlotId,
                        Date = request.AppointmentDate.Date,
                        Status = RoomTimeSlotStatus.Available
                    };
                    _context.RoomTimeSlot.Add(availableRoomTimeSlot);
                    await _context.SaveChangesAsync(); // Save RoomTimeSlot trước
                }
            }

            if (availableRoomTimeSlot == null)
            {
                throw new Exception("Không còn phòng available cho thời gian này");
            }

            // Đặt status thành Booked
            availableRoomTimeSlot.Status = RoomTimeSlotStatus.Booked;
            _context.RoomTimeSlot.Update(availableRoomTimeSlot);

            // Tạo appointment
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = userId,
                AppointmentDate = request.AppointmentDate,
                CreatedDate = DateTime.UtcNow,
                RoomTimeSlotId = availableRoomTimeSlot.Id,
                Status = AppointmentStatus.Scheduled,
                Note = request.Note
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
        }
    }
}
