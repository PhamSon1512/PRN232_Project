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

            var departmentId = Guid.Parse("46DFF497-0A45-4D22-AFBF-13B417B62F54");
            var email = "doctor@mediappointment.com";

            // Tìm Doctor theo email
            var doctor = await dbContext.Doctors
                                        .Include(d => d.DoctorDepartments)
                                        .FirstOrDefaultAsync(d => d.Email == email);

            if (doctor == null)
            {
                Console.WriteLine("Not Found: " + email);
                return;
            }

            var department = await dbContext.Departments.FirstOrDefaultAsync(d => d.Id == departmentId);
            if (department == null)
            {
                Console.WriteLine("NotFound Department with ID: " + departmentId);
                return;
            }

 
            var isAlreadyAssigned = await dbContext.DoctorDepartments
                .AnyAsync(dd => dd.DoctorId == doctor.Id && dd.DepartmentId == departmentId);

            if (!isAlreadyAssigned)
            {
                dbContext.DoctorDepartments.Add(new DoctorDepartment
                {
                    DoctorId = doctor.Id,             
                    DepartmentId = department.Id
                });

                await dbContext.SaveChangesAsync();
                Console.WriteLine(" Success.");
            }
            else
            {
                Console.WriteLine("Error Doctor in already in Department.");
            }
        }
    }
}
