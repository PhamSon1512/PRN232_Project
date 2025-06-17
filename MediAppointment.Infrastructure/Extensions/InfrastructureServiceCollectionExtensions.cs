using MediAppointment.Application.Interfaces;
using MediAppointment.Infrastructure.Persistence;
using MediAppointment.Infrastructure.Persistence.Configurations;
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

            // Đăng ký cấu hình EmailConfig
            services.Configure<EmailConfig>(configuration.GetSection("EmailConfig"));

            // Đăng ký EmailService
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
