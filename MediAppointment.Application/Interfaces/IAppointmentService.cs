using MediAppointment.Application.DTOs.AppointmentDTOs;
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
    }
}
