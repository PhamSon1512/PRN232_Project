using MediAppointment.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using MediAppointment.Application.DTOs.ManagerDTOs;
using MediAppointment.Application.DTOs.Pages;

namespace MediAppointment.Application.Interfaces
{
    public interface IAdminService
    {
        Task<PagedResult<DoctorManagerDto>> GetAllDoctorsAndManagersAsync(string text = "", int page = 1, int pageSize = 5);
        Task<object> GetAdminProfileAsync(Guid adminId);
        Task<DoctorManagerDto> GetUserByIdAsync(Guid id);
        Task<object> CreateDoctorToManagerAsync(ManagerCreateDto dto);
        Task<object> UpdateManagerProfileAsync(ManagerUpdateDto dto);
        Task<object> UpdateAdminProfileAsync(AdminUpdateProfileDto dto);
    }
}
