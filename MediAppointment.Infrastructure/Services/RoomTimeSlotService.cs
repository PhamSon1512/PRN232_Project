using AutoMapper;
using MediAppointment.Application.DTOs.AppointmentDTOs;
using MediAppointment.Application.DTOs.RoomTimeSlotDTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Enums;
using MediAppointment.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

using System.Globalization;


namespace MediAppointment.Infrastructure.Services
{
    public class RoomTimeSlotService : IRoomTimeSlotService
    {
        private readonly IGenericRepository<RoomTimeSlot> _roomTimeSlotRepository;
        private readonly IMapper _mapper;

        public RoomTimeSlotService(IGenericRepository<RoomTimeSlot> roomTimeSlotRepository, IMapper mapper)
        {
            _roomTimeSlotRepository = roomTimeSlotRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy danh sách ca khám (RoomTimeSlot) đã được phân công cho bác sĩ theo tuần và năm.
        /// Nếu không truyền vào, sẽ mặc định lấy tuần hiện tại.
        /// </summary>
        public async Task<IEnumerable<RoomTimeSlotResponse>> GetAssignedSlotsByDoctor(Guid doctorId, int? year = null, int? week = null)
        {
            var calendar = CultureInfo.InvariantCulture.Calendar;

            var now = DateTime.Today;
            int currentYear = year ?? now.Year;
            int currentWeek = week ?? calendar.GetWeekOfYear(now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            var rawData = await _roomTimeSlotRepository.Entities
                .Include(r => r.Room)
                .Include(r => r.TimeSlot)
                .Where(r => r.DoctorId == doctorId && r.Date.Year == currentYear)
                .ToListAsync(); // lấy dữ liệu từ DB trước

            return _mapper.Map<List<RoomTimeSlotResponse>>(rawData);
        }


        /// <summary>
        /// Lấy thông tin chi tiết của một RoomTimeSlot theo ID.
        /// </summary>
        public async Task<RoomTimeSlotResponse?> GetByIdAsync(Guid roomTimeSlotId)
        {
            var slot = await _roomTimeSlotRepository.Entities
                .Include(r => r.Room)
                .Include(r => r.TimeSlot)
                .FirstOrDefaultAsync(r => r.Id == roomTimeSlotId);

            return slot == null ? null : _mapper.Map<RoomTimeSlotResponse>(slot);
        }

        /// <summary>
        /// Cập nhật trạng thái của RoomTimeSlot (ví dụ: Available, Booked, Cancelled...).
        /// </summary>
        public async Task UpdateStatusAsync(Guid roomTimeSlotId, RoomTimeSlotStatus newStatus)
        {
            var slot = await _roomTimeSlotRepository.GetByIdAsync(roomTimeSlotId);
            if (slot == null)
                throw new Exception("RoomTimeSlot not found");

            slot.Status = newStatus;
            await _roomTimeSlotRepository.UpdateAsync(slot);
            await _roomTimeSlotRepository.SaveChangeAsync();
        }

        /// <summary>
        /// Lấy danh sách các năm mà có RoomTimeSlot trong hệ thống (dùng cho filter).
        /// </summary>
        public async Task<IEnumerable<int>> GetAvailableYears()
        {
            return await _roomTimeSlotRepository.Entities
                .Select(r => r.Date.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách tuần có dữ liệu trong một năm cụ thể (dùng để lọc theo tuần).
        /// </summary>
        public async Task<IEnumerable<int>> GetAvailableWeeksByYear(int year)
        {
            return await _roomTimeSlotRepository.Entities
                .Where(r => r.Date.Year == year)
                .Select(r => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                    r.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                .Distinct()
                .OrderBy(w => w)
                .ToListAsync();
        }

        public async Task<RoomTimeSlotDetailResponse?> GetDetailWithAppointmentsAsync(Guid roomTimeSlotId)
        {
            var slot = await _roomTimeSlotRepository.Entities
                .Include(r => r.Room)
                .Include(r => r.TimeSlot)
                .Include(r => r.Appointments)
                .FirstOrDefaultAsync(r => r.Id == roomTimeSlotId);

            if (slot == null)
                return null;

            var start = slot.TimeSlot.TimeStart;
            var duration = slot.TimeSlot.Duration;
            var end = start + duration;

            var response = new RoomTimeSlotDetailResponse
            {
                Id = slot.Id,
                Date = slot.Date,
                Status = slot.Status,
                RoomName = slot.Room?.Name ?? "N/A",
                TimeStart = start.ToString(@"hh\:mm"),
                TimeEnd = end.ToString(@"hh\:mm"),
                Duration = duration.ToString(@"hh\:mm"),
                Shift = slot.TimeSlot.Shift ? "Afternoon" : "Morning",
                Appointments = slot.Appointments?
                    .OrderBy(a => a.AppointmentDate)
                    .Select(a => _mapper.Map<AppointmentResponse>(a))
                    .ToList() ?? new()
            };

            return response;
        }

    }
}
