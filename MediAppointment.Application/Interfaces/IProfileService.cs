using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediAppointment.Domain.Entities;

namespace MediAppointment.Application.Interfaces
{
    public interface IProfileService
    {
        Task<Doctor?> GetProfileByIdAsync(Guid userIdentityId);
    }
}
