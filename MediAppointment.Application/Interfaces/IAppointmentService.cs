using MediAppointment.Application.DTOs.AppointmentDTOs;
using MediAppointment.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task CreateAppointment(Guid UserId, CreateAppointmentRequest request);

        Task<IEnumerable<AppointmentResponse>> ListAppointmentByUser(Guid UserId);

        Task<AppointmentResponse> AppointmentDetailById(Guid AppointmentId);
        
        Task CancelById(Guid AppointmentId);

        Task<IEnumerable<TimeSlotExsitResponse>> GetTimeSlotExsit(GetTimeSlotExistDTO request);


        // liệt kê lịch hẹn của bác sĩ
        Task<IEnumerable<AppointmentResponse>> ListAppointmentsAssignedToDoctor(
         Guid doctorId, DateTime? date = null, DateTime? startDate = null, DateTime? endDate = null);

        // Lấy danh sách khoa
        Task<IEnumerable<DepartmentResponse>> GetDepartments();

        // Lấy thông tin chi tiết về availability của time slots cho booking
        Task<IEnumerable<TimeSlotAvailabilityResponse>> GetAvailableTimeSlotsForBooking(GetTimeSlotExistDTO request, Guid? userId = null);

        // Book appointment với thông tin chi tiết hơn
        Task BookAppointment(Guid userId, BookAppointmentRequest request);
    }
}
