using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.DoctorDTOs;
using MediAppointment.Application.DTOs.ManagerDTOs;
using MediAppointment.Application.DTOs.Pages;
using MediAppointment.Domain.Entities;

namespace MediAppointment.Application.Interfaces
{
    public interface IManagerService
    {
        Task<PagedResult<DoctorDto>> GetAllDoctorsAsync(string text = "",/* string department = "",*/ int page = 1, int pageSize = 5);
        Task<Guid> CreateDoctorAsync(DoctorCreateDto dto);
        Task DeleteDoctorAsync(Guid doctorId);
        Task<DoctorDto> GetDoctorByIdAsync(Guid doctorId);
        Task<Doctor> ManagerUpdateDoctorAsync(Guid doctorId, ManagerDoctorUpdateDTO dto);
        Task<ManagerProfileDto> GetManagerProfileAsync(Guid userIdentityId);
        Task<bool> UpdateManagerProfileAsync(ManagerUpdateProfileDto dto);
    }
}
