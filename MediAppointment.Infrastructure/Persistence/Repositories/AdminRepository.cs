using MediAppointment.Domain.Interfaces;
using MediAppointment.Infrastructure.Data;
using System;
using System.Threading.Tasks;
using MediAppointment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MediAppointment.Infrastructure.Identity;

namespace MediAppointment.Infrastructure.Persistence.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;
        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateManagerAsync(string email, string fullName, string phoneNumber, string password)
        {
            var manager = new UserIdentity
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                FullName = fullName,
                PhoneNumber = phoneNumber
            };
            _context.Users.Add(manager);
            await _context.SaveChangesAsync();
            // Gán role Manager và set status nếu cần
            return manager.Id;
        }

        public async Task UpdateManagerRoleAsync(Guid managerId, string newRole)
        {
            var manager = await _context.Users.FindAsync(managerId);
            if (manager == null) throw new Exception("Manager not found");
            // Cập nhật role, nếu dùng Identity thì nên dùng UserManager để update role
            // ...
        }

        public async Task UpdateManagerStatusAsync(Guid managerId, Status status)
        {
            var manager = await _context.Users.FindAsync(managerId);
            if (manager == null) throw new Exception("Manager not found");
            // Nếu UserIdentity có property Status thì update trực tiếp
            // manager.Status = status;
            // await _context.SaveChangesAsync();
            // Nếu dùng claim thì cần update claim
            // ...
        }
    }
}
