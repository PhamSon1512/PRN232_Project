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
                        Id = Guid.Parse("0A438320-EEFE-4DB7-9DCA-51A17AB9AEE5"),
                        DepartmentName = "Cardiology"
                    },
                    new Department
                    {
                        Id = Guid.Parse("4D484610-3623-465D-82C2-55C0337A5A11"),
                        DepartmentName = "Neurology"
                    },
                    new Department
                    {
                        Id = Guid.Parse("E053F9BF-3195-4A42-904A-E99A615BE701"),
                        DepartmentName = "Pediatrics"
                    }
                };

                dbContext.Departments.AddRange(departments);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
