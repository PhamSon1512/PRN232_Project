using MediAppointment.Application.DTOs.BookingDoctorDTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediAppointment.Application.Interfaces
{
    public interface IAppointmentBookingDoctor
    {
        Task CreateAsync(BookingDoctorCreate request);

        Task UpdateAsync(Guid appointmentId, BookingDoctorUpdate request);

        Task CancelAsync(Guid appointmentId, Guid patientId);

        Task<BookingDoctorResponse> GetByIdAsync(Guid appointmentId);

        Task<IEnumerable<BookingDoctorResponse>> GetByDoctorAsync(Guid doctorId);

        Task<IEnumerable<BookingDoctorResponse>> GetAllAsync();

        Task<IEnumerable<BookingDoctorResponse>> GetByPatientAsync(Guid patientId);

        // ✅ NEW: Cập nhật trạng thái (duyệt hoặc từ chối)
        Task UpdateStatusAsync(Guid appointmentId, Guid doctorId, BookingDoctorStatusUpdate request);
    }

}
