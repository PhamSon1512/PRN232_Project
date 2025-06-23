using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediAppointment.Application.DTOs;
using MediAppointment.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.Application.Interfaces
{
    public interface IProfileService
    {
        Task<Doctor?> GetProfileByIdAsync(Guid userIdentityId);
        Task<Doctor> UpdateProfileAsync(DoctorUpdateDto dto);
    }
}
