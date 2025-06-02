using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Interfaces;
using MediAppointment.Infrastructure.Persistence.Repositories;

namespace MediAppointment.Infrastructure.Persistence
{
    public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private Hashtable _repositories = [];

        public IGenericRepository<T> Repository<T>() where T : Entity
        {
            _repositories ??= [];
            var type = typeof(T).Name;
            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);

                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _dbContext);

                _repositories.Add(type, repositoryInstance);
            }
            return (IGenericRepository<T>)_repositories[type]!;
        }

        public Task Rollback()
        {
            _dbContext.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
            return Task.CompletedTask;
        }

        public async Task<int> Save(CancellationToken cancellationToken)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
