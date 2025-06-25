    using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Interfaces;
using MediAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediAppointment.Infrastructure.Persistence.Repositories
{
    public class ProfileRepository : GenericRepository<Doctor>, IProfileRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProfileRepository(ApplicationDbContext dbContext, IUnitOfWork unitOfWork) : base(dbContext)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Doctor?> GetProfileByIdAsync(Guid userIdentityId)
        {
            return await _unitOfWork.Repository<Doctor>().GetByUserIdentityIdAsync(userIdentityId);
        }

        public async Task<Doctor> UpdateProfileAsync(Doctor doctor, CancellationToken cancellationToken = default)
        {
            await UpdateAsync(doctor);
            await _unitOfWork.Save(cancellationToken);
            return doctor;
        }
    }
}