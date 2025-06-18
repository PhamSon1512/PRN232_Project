using MediAppointment.Domain.Entities;
using MediAppointment.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Infrastructure.Persistence.Seeder
{
    public static class DepartmentSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (!dbContext.Departments.Any())
            {
                var departments = new List<Department>
                {
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Cardiology"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Neurology"
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        DepartmentName = "Pediatrics"
                    }
                };

                dbContext.Departments.AddRange(departments);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
