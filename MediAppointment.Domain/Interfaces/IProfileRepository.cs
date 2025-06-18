using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediAppointment.Domain.Entities;

namespace MediAppointment.Domain.Interfaces
{
    public interface IProfileRepository
    {
        Task<Doctor?> GetProfileByIdAsync(Guid userIdentityId);

        //Task<Doctor> CreateProfileAsync(DoctorCreateDto dto);
        //Task<Doctor> UpdateProfileAsync(DoctorUpdateDto dto);
        //Task DeleteProfileAsync(Guid userIdentityId);
    }
}
