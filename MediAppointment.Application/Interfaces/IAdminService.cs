using MediAppointment.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace MediAppointment.Application.Interfaces
{
    public interface IAdminService
    {
        // Quản lý Manager (tạo, cập nhật role/status, chuyển trạng thái Deleted)
        Task<Guid> CreateManagerAsync(string email, string fullName, string phoneNumber, string password);
        Task UpdateManagerRoleAsync(Guid managerId, string newRole);
        Task UpdateManagerStatusAsync(Guid managerId, MediAppointment.Domain.Enums.Status status);
    }
}
