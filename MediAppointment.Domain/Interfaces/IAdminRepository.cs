using System;
using System.Threading.Tasks;
using MediAppointment.Domain.Enums;

namespace MediAppointment.Domain.Interfaces
{
    public interface IAdminRepository
    {
        // Quản lý Manager
        Task<Guid> CreateManagerAsync(string email, string fullName, string phoneNumber, string password);
        Task UpdateManagerRoleAsync(Guid managerId, string newRole);
        Task UpdateManagerStatusAsync(Guid managerId, Status status);
    }
}
