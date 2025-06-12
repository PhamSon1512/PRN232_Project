using MediAppointment.Domain.Entities;
using MediAppointment.Infrastructure.Data;
using MediAppointment.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MediAppointment.Infrastructure.Persistence.Seeder
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<UserIdentity>>();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Seed Doctor UserIdentity + Doctor domain
            var doctorEmail = "doctor@mediappointment.com";
            var doctorUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == doctorEmail);
            if (doctorUser == null)
            {
                doctorUser = new UserIdentity
                {
                    UserName = doctorEmail,
                    Email = doctorEmail,
                    FullName = "Default Doctor",
                    PhoneNumber = "0987654321",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(doctorUser, "Doctor@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(doctorUser, "Doctor");

                    // Tạo Doctor domain entity và liên kết UserIdentityId
                    var doctor = new Doctor
                    {
                        Id = Guid.NewGuid(),
                        FullName = doctorUser.FullName ?? "",
                        Gender = true,
                        DateOfBirth = new DateTime(1990, 1, 1),
                        Email = doctorUser.Email ?? "",
                        PhoneNumber = doctorUser.PhoneNumber ?? ""
                    };
                    dbContext.Doctors.Add(doctor);
                    dbContext.Entry(doctor).Property("UserIdentityId").CurrentValue = doctorUser.Id;
                    await dbContext.SaveChangesAsync();
                }
            }

            // 2. Seed Patient UserIdentity + Patient domain
            var patientEmail = "patient@mediappointment.com";
            var patientUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == patientEmail);
            if (patientUser == null)
            {
                patientUser = new UserIdentity
                {
                    UserName = patientEmail,
                    Email = patientEmail,
                    FullName = "Default Patient",
                    PhoneNumber = "0111222333",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(patientUser, "Patient@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(patientUser, "Patient");

                    // Tạo Patient domain entity và liên kết UserIdentityId
                    var patient = new Patient
                    {
                        Id = Guid.NewGuid(),
                        FullName = patientUser.FullName ?? "",
                        Gender = false,
                        DateOfBirth = new DateTime(1995, 5, 5),
                        Email = patientUser.Email ?? "",
                        PhoneNumber = patientUser.PhoneNumber ?? "",
                        CCCD = "012345678901",
                        Address = "123 Main St",
                        BHYT = "BHYT123456"
                    };
                    dbContext.Patients.Add(patient);
                    dbContext.Entry(patient).Property("UserIdentityId").CurrentValue = patientUser.Id;
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
