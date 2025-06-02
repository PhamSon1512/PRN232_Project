using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediAppointment.Application.Mapper;
using Microsoft.Extensions.DependencyInjection;

namespace MediAppointment.Application.Extensions
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = typeof(ApplicationServiceCollectionExtensions).Assembly;

            services.AddAutoMapper(assembly);

            services.AddValidatorsFromAssembly(assembly)
                .AddFluentValidationAutoValidation();

            return services;
        }
    }
}
