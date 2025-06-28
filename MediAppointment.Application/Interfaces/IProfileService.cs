using MediAppointment.Application.DTOs;
using MediAppointment.Domain.Entities;

namespace MediAppointment.Application.Interfaces
{
    public interface IProfileService
    {
        Task<Doctor?> GetProfileByIdAsync(Guid userIdentityId);
        Task<Doctor> UpdateProfileAsync(Guid userId, DoctorUpdateDto dto);
    }
}
