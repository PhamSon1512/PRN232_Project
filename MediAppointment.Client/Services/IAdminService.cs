using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediAppointment.Client.Models;

namespace MediAppointment.Client.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<UserViewModel>> GetAllUsersAsync();
        Task SetUserStatusAsync(Guid id, bool isActive);
        Task ChangeUserRoleAsync(Guid id, string newRole);
    }
}
