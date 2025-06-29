using MediAppointment.Domain.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediAppointment.Infrastructure.Persistence.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;
        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Admin>> GetAllAsync()
        {
            return await _context.Admins.ToListAsync();
        }

        public async Task<Admin?> GetByIdAsync(Guid id)
        {
            return await _context.Admins.FindAsync(id);
        }

        public async Task AddAsync(Admin admin)
        {
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Admin admin)
        {
            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin != null)
            {
                _context.Admins.Remove(admin);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            // Trả về tất cả các loại user cụ thể (Admin, Doctor, Patient, ...)
            var admins = await _context.Admins.ToListAsync<User>();
            var doctors = await _context.Doctors.ToListAsync<User>();
            var patients = await _context.Patients.ToListAsync<User>();
            return admins.Concat(doctors).Concat(patients);
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Admins.FirstOrDefaultAsync(x => x.Id == id)
                ?? (User?)await _context.Doctors.FirstOrDefaultAsync(x => x.Id == id)
                ?? await _context.Patients.FirstOrDefaultAsync(x => x.Id == id);
            return user;
        }

        public async Task SetUserStatusAsync(Guid userId, bool isActive)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.Status = isActive ? Domain.Enums.Status.Active : Domain.Enums.Status.Inactive;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ChangeUserRoleAsync(Guid userId, string newRole)
        {
            // Nếu có property Role ở các entity, hãy cập nhật ở đây
            // Ví dụ: nếu user là Doctor hoặc Manager thì đổi role
            // Cần bổ sung logic cụ thể tuỳ vào thiết kế
        }
    }
}
