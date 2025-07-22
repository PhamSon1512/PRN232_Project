using AutoMapper;
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
                CreatedAt = DateTime.UtcNow
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
            booking.UpdatedAt = DateTime.UtcNow;

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
            booking.UpdatedAt = DateTime.UtcNow;

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
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserIdentityId == userIdentityDoctorId);
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
                .Where(p => patientUserIds.Contains(p.UserIdentityId.Value))
                .ToDictionaryAsync(p => p.UserIdentityId, p => p.FullName);

            var departmentIds = bookings.Select(b => b.DepartmentId).Distinct().ToList();
            var departments = await _context.Departments
                .Where(d => departmentIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.DepartmentName);

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
                PatientName = patients.TryGetValue(b.PatientId, out var patientName) ? patientName : null
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
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserIdentityId == doctorId);
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
                .FirstOrDefaultAsync(p => p.UserIdentityId == patientUserIdentityId);
            var patientName = patient?.FullName;

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
                PatientName = patientName
            });

            return result;
        }


        public async Task UpdateStatusAsync(Guid appointmentId, Guid doctorId, BookingDoctorStatusUpdate request)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserIdentityId == doctorId);
            var booking = await _context.AppointmentBookingDoctors
                .FirstOrDefaultAsync(x => x.Id == appointmentId && x.DoctorId == doctor.Id);

            if (booking == null)
                throw new Exception("Không tìm thấy lịch hẹn.");

            booking.Status = request.Status;
            booking.Note = request.Note;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

    }
}
