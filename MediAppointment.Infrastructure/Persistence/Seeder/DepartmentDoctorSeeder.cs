using MediAppointment.Domain.Entities;
using MediAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Infrastructure.Persistence.Seeder
{
    public static class DepartmentDoctorSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var random = new Random();

            var departments = await dbContext.Departments.ToListAsync();
            var doctors = await dbContext.Doctors.ToListAsync();

            if (!doctors.Any() || !departments.Any())
            {
                Console.WriteLine("⚠️ Không có Doctor hoặc Department để seed.");
                return;
            }

            foreach (var doctor in doctors)
            {
                // Random 2-3 Department cho mỗi Doctor
                var randomDepartments = departments
                    .OrderBy(_ => random.Next())
                    .Take(random.Next(2, 4));

                foreach (var department in randomDepartments)
                {
                    bool alreadyAssigned = await dbContext.DoctorDepartments
                        .AnyAsync(dd => dd.DoctorId == doctor.Id && dd.DepartmentId == department.Id);

                    if (!alreadyAssigned)
                    {
                        dbContext.DoctorDepartments.Add(new DoctorDepartment
                        {
                            DoctorId = doctor.Id,
                            DepartmentId = department.Id
                        });
                        Console.WriteLine($"✅ Assigned Doctor ({doctor.FullName}) to Department ({department.DepartmentName})");
                    }
                    else
                    {
                        Console.WriteLine($"ℹ️ Doctor ({doctor.FullName}) already assigned to Department ({department.DepartmentName})");
                    }
                }
            }

            await dbContext.SaveChangesAsync();
            Console.WriteLine("🎉 Completed seeding Doctor-Department relationships.");
        }
    }
}

