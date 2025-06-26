using MediAppointment.Application.Extensions;
using MediAppointment.Infrastructure.Extensions;
using MediAppointment.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;

namespace MediAppointment.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {

            // Registering application services
            services.AddApplicationServices();

            // Registering infrastructure services
            services.AddInfrastructureServices(configuration);

            // Add API-specific services here, such as controllers, Swagger, etc.
            services.AddControllers();

            //var jwtConfig = configuration.GetSection("Jwt");
            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //})
            //.AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = true,
            //        ValidateLifetime = true,
            //        ValidateIssuerSigningKey = true,
            //        ValidIssuer = jwtConfig["Issuer"],
            //        ValidAudience = jwtConfig["Audience"],
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]))
            //    };
            //});
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "BearerAuth"}
                        },
                        []
                    }
                });
            });

            services.AddEndpointsApiExplorer();



            return services;
        }
    }
}
