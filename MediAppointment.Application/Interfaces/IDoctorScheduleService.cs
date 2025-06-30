using MediAppointment.Application.DTOs.DoctorScheduleDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.Interfaces
{
    public interface IDoctorScheduleService
    {

        public Task CreateDoctorSchedule(Guid DoctorId, List<DoctorScheduleRequest> request);
        Task DeleteDoctorSchedule(Guid DoctorId,DeleteDoctorScheduleDTO request);
        Task<List<DoctorScheduleResponse>> GetDoctorSchedule(DoctorScheduleRequestDTO request);
    }
}
