using MediAppointment.Application.Extensions;
using MediAppointment.Infrastructure.Persistence;

namespace MediAppointment.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {

            // Registering application services
            services.AddApplicationServices();

            // Registering persistence services
            services.AddPersistenceServices(configuration);

            // Add API-specific services here, such as controllers, Swagger, etc.
            services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }
    }
}
