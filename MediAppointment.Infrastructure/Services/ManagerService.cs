using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.DoctorDTOs;
using MediAppointment.Application.DTOs.Pages;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Entities;
using MediAppointment.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediAppointment.Domain.Interfaces;
using MediAppointment.Application.DTOs.ManagerDTOs;
using System.Globalization;
using MediAppointment.Infrastructure.Data;

namespace MediAppointment.Infrastructure.Services
{
    public class ManagerService : IManagerService
    {
        private readonly UserManager<UserIdentity> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IIdentityService _identityService;
        private readonly IProfileRepository _profileRepository;

        public ManagerService(UserManager<UserIdentity> userManager, ApplicationDbContext context ,IEmailService emailService, IIdentityService identityService, IProfileRepository profileRepository)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _identityService = identityService;
            _profileRepository = profileRepository;
        }

        public async Task<PagedResult<DoctorDto>> GetAllDoctorsAsync(string text = "", /*string department = "",*/ int page = 1, int pageSize = 5)
        {
            return await _identityService.GetAllDoctorsAsync(text, /*department,*/ page, pageSize);
        }

        public async Task<ManagerProfileDto> GetManagerProfileAsync(Guid userIdentityId)
        {
            return await _identityService.GetManagerProfileAsync(userIdentityId);
        }

        public async Task<bool> UpdateManagerProfileAsync(ManagerUpdateProfileDto dto)
        {
            return await _identityService.UpdateManagerProfileAsync(dto);
        }

        public async Task<DoctorDto> GetDoctorByIdAsync(Guid doctorId)
        {
            return await _identityService.GetDoctorByIdAsync(doctorId);
        }

        public async Task<DoctorDto> ManagerUpdateDoctorAsync(Guid doctorId, ManagerDoctorUpdateDTO dto)
        {
            return await _identityService.ManagerUpdateDoctorAsync(doctorId, dto);
        }

        public async Task<Guid> CreateDoctorAsync(DoctorCreateDto dto)
        {
            return await _identityService.CreateDoctorAsync(dto);
        }

        public async Task DeleteDoctorAsync(Guid doctorId)
        {
            await _identityService.DeleteDoctorAsync(doctorId);
        }

        public async Task<Dictionary<DateTime, List<ManagerScheduleSlot>>> GetWeeklyScheduleAsync(Guid? departmentId, Guid? roomId, Guid? doctorId, int year, int week)
        {
            try
            {
                // Kiểm tra đầu vào
                if (year < 2000 || year > DateTime.Now.Year + 1)
                    throw new ArgumentException("Năm không hợp lệ.");
                if (week < 1 || week > 53)
                    throw new ArgumentException("Tuần không hợp lệ.");

                var calendar = CultureInfo.InvariantCulture.Calendar;
                var jan1 = new DateTime(year, 1, 1);
                var daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
                if (daysOffset > 0) daysOffset -= 7; // Đảm bảo lấy thứ Hai đầu tiên
                var firstMonday = jan1.AddDays(daysOffset);
                var startOfWeek = firstMonday.AddDays((week - 1) * 7);
                var weekDates = Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i).Date).ToList();

                var query = _context.RoomTimeSlot
                    .Include(rts => rts.Room).ThenInclude(r => r.Department)
                    .Include(rts => rts.TimeSlot)
                    .Include(rts => rts.Doctor)
                    .Include(rts => rts.Appointments)
                    .Where(rts => weekDates.Contains(rts.Date.Date) && rts.DoctorId != null);

                // Áp dụng bộ lọc
                if (departmentId.HasValue && departmentId != Guid.Empty)
                {
                    query = query.Where(rts => rts.Room.DepartmentId == departmentId);
                }

                if (roomId.HasValue && roomId != Guid.Empty)
                {
                    query = query.Where(rts => rts.RoomId == roomId);
                }

                if (doctorId.HasValue && doctorId != Guid.Empty)
                {
                    query = query.Where(rts => rts.DoctorId == doctorId);
                }

                var slots = await query
                    .Select(rts => new
                    {
                        rts.Id,
                        TimeStart = rts.TimeSlot.TimeStart,
                        TimeEnd = rts.TimeSlot.TimeEnd,
                        DoctorName = rts.Doctor != null ? rts.Doctor.FullName : "Không có bác sĩ",
                        RoomName = rts.Room != null ? rts.Room.Name : "Không có phòng",
                        DepartmentName = rts.Room != null && rts.Room.Department != null ? rts.Room.Department.DepartmentName : "Không có khoa",
                        rts.Date,
                        Shift = rts.TimeSlot.Shift,
                        AppointmentCount = rts.Appointments != null ? rts.Appointments.Count : 0
                    })
                    .ToListAsync();

                var weeklySchedule = new Dictionary<DateTime, List<ManagerScheduleSlot>>();

                foreach (var date in weekDates)
                {
                    var dailySlots = slots
                        .Where(s => s.Date.Date == date)
                        .Select(s => new ManagerScheduleSlot
                        {
                            Id = s.Id,
                            TimeRange = $"{s.TimeStart:hh\\:mm} - {s.TimeEnd:hh\\:mm}",
                            DoctorName = s.DoctorName,
                            RoomName = s.RoomName,
                            DepartmentName = s.DepartmentName,
                            Date = s.Date,
                            Period = s.Shift ? "chiều" : "sáng",
                            AppointmentCount = s.AppointmentCount,
                            MaxAppointments = 5, // TODO: Cấu hình từ cơ sở dữ liệu hoặc appsettings
                            IsFullyBooked = s.AppointmentCount >= 5,
                            IsRegistered = true // Vì đã lọc DoctorId != null
                        })
                        .OrderBy(s => s.TimeRange)
                        .ToList();

                    weeklySchedule[date] = dailySlots;
                }

                return weeklySchedule;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy lịch hàng tuần: {ex.Message}", ex);
            }
        }
    }
}
