using AutoMapper;
using Azure.Core;
using MediAppointment.Application.DTOs.BookingDoctorDTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Enums;
using MediAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediAppointment.Infrastructure.Services
{
    public class BookingDoctorService : IAppointmentBookingDoctor
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BookingDoctorService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task CreateAsync(BookingDoctorCreate request)
        {
            var booking = new AppointmentBookingDoctor
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                DepartmentId = request.DepartmentId,
                TimeSlotId = request.TimeSlotId,
                AppointmentDate = request.AppointmentDate,
                Note = request.Note,
                Status = "Pending", // Gán chuỗi trực tiếp
                CreatedAt = DateTime.UtcNow.AddHours(7)
            };

            _context.AppointmentBookingDoctors.Add(booking);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateAsync(Guid appointmentId, BookingDoctorUpdate request)
        {
            var booking = await _context.AppointmentBookingDoctors.FindAsync(appointmentId);

            if (booking == null)
                throw new Exception("Không tìm thấy lịch hẹn");

            booking.DoctorId = request.DoctorId;
            booking.DepartmentId = request.DepartmentId;
            booking.TimeSlotId = request.TimeSlotId;
            booking.AppointmentDate = request.AppointmentDate;
            booking.Note = request.Note;
            booking.Status = request.Status;
            booking.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _context.AppointmentBookingDoctors.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task CancelAsync(Guid appointmentId, Guid patientId)
        {
            var booking = await _context.AppointmentBookingDoctors
                .FirstOrDefaultAsync(x => x.Id == appointmentId && x.PatientId == patientId);

            if (booking == null)
                throw new Exception("Không tìm thấy lịch hẹn để hủy.");

            booking.Status = "Canceled"; // hoặc BookingStatus.Cancelled.ToString() nếu dùng enum
            booking.UpdatedAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();
        }


        public async Task<BookingDoctorResponse> GetByIdAsync(Guid appointmentId)
        {
            var booking = await _context.AppointmentBookingDoctors
                .FirstOrDefaultAsync(x => x.Id == appointmentId);

            if (booking == null)
                throw new Exception("Không tìm thấy lịch hẹn");

            return _mapper.Map<BookingDoctorResponse>(booking);
        }


        public async Task<IEnumerable<BookingDoctorResponse>> GetByDoctorAsync(Guid userIdentityDoctorId)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == userIdentityDoctorId);
            if (doctor == null)
                return Enumerable.Empty<BookingDoctorResponse>();

            var bookings = await _context.AppointmentBookingDoctors
                .Where(b => b.DoctorId == doctor.Id)
                .OrderByDescending(b => b.AppointmentDate)
                .ToListAsync();

            var patientUserIds = bookings
                .Select(b => b.PatientId)
                .Distinct()
                .ToList();

            var patients = await _context.Patients
                .Where(p => patientUserIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.FullName);

            var departmentIds = bookings.Select(b => b.DepartmentId).Distinct().ToList();
            var departments = await _context.Departments
                .Where(d => departmentIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.DepartmentName);

            var timeSlotIds = bookings.Select(b => b.TimeSlotId).Distinct().ToList();
            var timeSlots = await _context.TimeSlot
                .Where(t => timeSlotIds.Contains(t.Id))
                .ToDictionaryAsync(
                    t => t.Id,
                    t => new { t.TimeStart, t.Duration }
                );


            var result = bookings.Select(b => new BookingDoctorResponse
            {
                Id = b.Id,
                PatientId = b.PatientId,
                DoctorId = b.DoctorId,
                DepartmentId = b.DepartmentId,
                TimeSlotId = b.TimeSlotId,
                AppointmentDate = b.AppointmentDate,
                Note = b.Note,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,

                DoctorName = doctor.FullName,
                DepartmentName = departments.TryGetValue(b.DepartmentId, out var deptName) ? deptName : null,
                PatientName = patients.TryGetValue(b.PatientId, out var patientName) ? patientName : null,
                TimeRange = timeSlots.TryGetValue(b.TimeSlotId, out var slot)
                    ? $"{slot.TimeStart.ToString(@"hh\:mm")} - {(slot.TimeStart + slot.Duration).ToString(@"hh\:mm")}"
                    : string.Empty
            });

            return result;
        }



        public async Task<IEnumerable<BookingDoctorResponse>> GetAllAsync()
        {
            var bookings = await _context.AppointmentBookingDoctors.ToListAsync();
            return _mapper.Map<IEnumerable<BookingDoctorResponse>>(bookings);
        }

        public async Task UpdateBookingStatusAsync(Guid appointmentId, Guid doctorId, BookingDoctorStatusUpdate request)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            var booking = await _context.AppointmentBookingDoctors
                .FirstOrDefaultAsync(x => x.Id == appointmentId && x.DoctorId == doctor.Id);

            if (booking == null)
                throw new Exception("Không tìm thấy lịch hẹn.");

            if (request.Status != "Approved" && request.Status != "Rejected")
                throw new Exception("Trạng thái không hợp lệ.");

            booking.Status = request.Status;
            booking.Note = request.Status == "Rejected" ? request.Note : null;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<BookingDoctorResponse>> GetByPatientAsync(Guid patientUserIdentityId)
        {
            var bookings = await _context.AppointmentBookingDoctors
                .Where(b => b.PatientId == patientUserIdentityId)
                .OrderByDescending(b => b.AppointmentDate)
                .ToListAsync();

            if (!bookings.Any())
                return Enumerable.Empty<BookingDoctorResponse>();

            var doctorIds = bookings.Select(b => b.DoctorId).Distinct().ToList();
            var departmentIds = bookings.Select(b => b.DepartmentId).Distinct().ToList();

            var doctors = await _context.Doctors
                .Where(d => doctorIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.FullName);

            var departments = await _context.Departments
                .Where(d => departmentIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.DepartmentName);

            // Lấy tên bệnh nhân từ bảng Patients, join bằng UserIdentityId
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == patientUserIdentityId);
            var patientName = patient?.FullName;

            var timeSlotIds = bookings.Select(b => b.TimeSlotId).Distinct().ToList();
            var timeSlots = await _context.TimeSlot
                .Where(t => timeSlotIds.Contains(t.Id))
                .ToDictionaryAsync(
                    t => t.Id,
                    t => new { t.TimeStart, t.Duration }
                );

            var result = bookings.Select(b => new BookingDoctorResponse
            {
                Id = b.Id,
                PatientId = b.PatientId,
                DoctorId = b.DoctorId,
                DepartmentId = b.DepartmentId,
                TimeSlotId = b.TimeSlotId,
                AppointmentDate = b.AppointmentDate,
                Note = b.Note,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,

                DoctorName = doctors.TryGetValue(b.DoctorId, out var docName) ? docName : null,
                DepartmentName = departments.TryGetValue(b.DepartmentId, out var deptName) ? deptName : null,
                PatientName = patientName,
                TimeRange = timeSlots.TryGetValue(b.TimeSlotId, out var slot)
                    ? $"{slot.TimeStart.ToString(@"hh\:mm")} - {(slot.TimeStart + slot.Duration).ToString(@"hh\:mm")}"
                    : string.Empty
            });

            return result;
        }


        public async Task UpdateStatusAsync(Guid appointmentId, Guid doctorId, BookingDoctorStatusUpdate request)
        {
            var booking = await _context.AppointmentBookingDoctors
                .FirstOrDefaultAsync(x => x.Id == appointmentId && x.DoctorId == doctorId);

            if (booking == null)
                throw new Exception("Không tìm thấy lịch hẹn.");

            if (request.Status == "Rejected")
            {
                booking.Status = request.Status;
                booking.Note = request.Note;
                booking.UpdatedAt = DateTime.UtcNow.AddHours(7);
                await _context.SaveChangesAsync();
                return;
            }

            var appointmentDate = request.AppointmentDate.Date;
            var timeSlotId = request.TimeSlotID;
            var departmentId = request.DepartmentId;

            // Lấy shift từ TimeSlotId
            var timeSlot = await _context.TimeSlot.FirstOrDefaultAsync(t => t.Id == timeSlotId);
            if (timeSlot == null)
                throw new Exception("Không tìm thấy khung giờ làm việc.");

            var shift = timeSlot.Shift;

            // Lấy danh sách tất cả các phòng trong khoa
            var roomIdsInDepartment = await _context.Room
                .Where(r => r.DepartmentId == departmentId)
                .Select(r => r.Id)
                .ToListAsync();

            if (!roomIdsInDepartment.Any())
                throw new Exception("Không tìm thấy phòng nào trong khoa được chọn.");

            // Tìm RoomTimeSlot có cùng ngày, cùng TimeSlotId trong các phòng đó
            var roomTimeSlots = await _context.RoomTimeSlot
                .Include(r => r.TimeSlot)
                .Include(r => r.Room)
                .Where(r =>
                    roomIdsInDepartment.Contains(r.RoomId) &&
                    r.TimeSlotId == timeSlotId &&
                    r.Date == appointmentDate)
                .ToListAsync();

            if (!roomTimeSlots.Any())
                throw new Exception("Không tìm thấy ca khám phù hợp trong khoa đã chọn.");

            // Ưu tiên tìm slot đã có bác sĩ hiện tại (tránh gán lại)
            var existingSlotWithDoctor = roomTimeSlots.FirstOrDefault(r => r.DoctorId == doctorId);
            if (existingSlotWithDoctor != null)
            {
                // Kiểm tra đã có bệnh nhân chưa
                bool hasPatient = await _context.Appointments.AnyAsync(a => a.RoomTimeSlotId == existingSlotWithDoctor.Id);
                if (hasPatient)
                    throw new Exception("Ca khám này đã có bệnh nhân khác đăng ký.");

                await CreateAppointmentAsync(request.PatientID, existingSlotWithDoctor.Id, appointmentDate, request.Note);
            }
            else
            {
                // Tìm một slot chưa có bác sĩ và chưa có bệnh nhân
                var availableSlot = roomTimeSlots.FirstOrDefault(r => r.DoctorId == null);
                if (availableSlot == null)
                    throw new Exception("Tất cả các phòng trong ca này đều đã có bác sĩ.");

                bool hasPatient = await _context.Appointments.AnyAsync(a => a.RoomTimeSlotId == availableSlot.Id);
                if (hasPatient)
                    throw new Exception("Tất cả các phòng trong ca này đều đã có bệnh nhân.");

                // Gán bác sĩ vào tất cả các slot cùng phòng, cùng buổi, cùng ngày
                var sameRoomSameShiftSlots = await _context.RoomTimeSlot
                    .Where(r => r.RoomId == availableSlot.RoomId
                                && r.Date == appointmentDate
                                && r.TimeSlot.Shift == shift)
                    .Include(r => r.TimeSlot)
                    .ToListAsync();

                foreach (var slot in sameRoomSameShiftSlots)
                {
                    slot.DoctorId = doctorId;
                }

                await CreateAppointmentAsync(request.PatientID, availableSlot.Id, appointmentDate, request.Note);
            }

            booking.Status = "Approved";
            booking.Note = request.Note;
            booking.UpdatedAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();
        }



        private async Task CreateAppointmentAsync(Guid patientId, Guid roomTimeSlotId, DateTime date, string? note)
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                AppointmentDate = date,
                Status = 0,
                Note = note,
                CreatedDate = DateTime.UtcNow.AddHours(7),
                UpdatedDate = DateTime.UtcNow.AddHours(7),
                RoomTimeSlotId = roomTimeSlotId
            };

            _context.Appointments.Add(appointment);
        }


    }
}
