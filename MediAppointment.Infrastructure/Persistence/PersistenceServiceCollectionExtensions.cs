using MediAppointment.Domain.Interfaces;
using MediAppointment.Infrastructure.Data;
using MediAppointment.Infrastructure.Identity;
using MediAppointment.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediAppointment.Infrastructure.Persistence
{
    public static class PersistenceServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration config)
        {
            var assembly = typeof(PersistenceServiceCollectionExtensions).Assembly;
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            // Đăng ký Identity cho UserIdentity và IdentityRole<Guid>
            services.AddIdentity<UserIdentity, IdentityRole<Guid>>(options =>
            {
                // Cấu hình password, lockout, v.v. nếu muốn
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            return services;
        }
    }
}
