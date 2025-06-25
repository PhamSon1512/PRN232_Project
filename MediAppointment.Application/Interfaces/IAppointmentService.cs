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


        // liệt kê lịch hẹn của bác sĩ
        Task<IEnumerable<AppointmentResponse>> ListAppointmentsAssignedToDoctor(
         Guid doctorId, DateTime? date = null, DateTime? startDate = null, DateTime? endDate = null);
    }
}
