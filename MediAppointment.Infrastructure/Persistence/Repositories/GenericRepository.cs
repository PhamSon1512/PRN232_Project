using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Interfaces;
using MediAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediAppointment.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : Entity
    {
        private readonly ApplicationDbContext _dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<T> Entities => _dbContext.Set<T>();

        public async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            return entity;
        }

        public Task UpdateAsync(T entity)
        {
            T exist = _dbContext.Set<T>().Find(entity.Id)
                ?? throw new Exception($"Entity of type {nameof(T)} with ID {entity.Id} not found.");
            _dbContext.Entry(exist).CurrentValues.SetValues(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbContext
                .Set<T>()
                .ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<T?> GetByUserIdentityIdAsync(Guid userIdentityId)
        {
            return await _dbContext.Set<T>()
                .FirstOrDefaultAsync(e => EF.Property<Guid?>(e, "UserIdentityId") == userIdentityId
                    && EF.Property<string>(e, "Discriminator") == typeof(T).Name);
        }

        public async Task SaveChangeAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
