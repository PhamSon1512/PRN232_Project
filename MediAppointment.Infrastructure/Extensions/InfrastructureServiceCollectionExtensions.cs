using MediAppointment.Application.Interfaces;
using MediAppointment.Infrastructure.Persistence;
using MediAppointment.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediAppointment.Infrastructure.Extensions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký các service liên quan đến database (Persistence)
            services.AddPersistenceServices(configuration);

            // Đăng ký các service hạ tầng khác
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<IVnPayService, VnPayService>();
            services.AddScoped<IWalletService, WalletService>();

            return services;
        }
    }
}
