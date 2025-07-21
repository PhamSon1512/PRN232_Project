using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Enums;
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
                    EmailConfirmed = true,
                    RefreshToken = string.Empty,
                    Status = Status.Active,
                    Gender = false,
                    DateOfBirth = new DateTime(1990, 1, 1)
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
                        Gender = doctorUser.Gender,
                        DateOfBirth = doctorUser.DateOfBirth,
                        Email = doctorUser.Email ?? "",
                        PhoneNumber = doctorUser.PhoneNumber ?? "",
                        Status = doctorUser.Status
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
                    EmailConfirmed = true,
                    RefreshToken = string.Empty,
                    Status = Status.Active,
                    Gender = false,
                    DateOfBirth = new DateTime(1995, 5, 5)
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
                        Gender = patientUser.Gender,
                        DateOfBirth = patientUser.DateOfBirth,
                        Email = patientUser.Email ?? "",
                        PhoneNumber = patientUser.PhoneNumber ?? "",
                        CCCD = "012345678901",
                        Address = "123 Main St",
                        BHYT = "BHYT123456",
                        Status = patientUser.Status
                    };
                    dbContext.Patients.Add(patient);
                    dbContext.Entry(patient).Property("UserIdentityId").CurrentValue = patientUser.Id;
                    await dbContext.SaveChangesAsync();
                }
            }

            // 3. Seed Manager UserIdentity without domain entity
            var managerEmail = "manager@mediappointment.com";
            var managerUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == managerEmail);
            if (managerUser == null)
            {
                managerUser = new UserIdentity
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    FullName = "Default Manager",
                    PhoneNumber = "0901234567",
                    EmailConfirmed = true,
                    RefreshToken = string.Empty,
                    RefreshTokenExpiryTime = DateTime.MinValue,
                    Status = Status.Active,
                    Gender = true,
                    DateOfBirth = new DateTime(1985, 3, 15)
                };
                var result = await userManager.CreateAsync(managerUser, "Manager@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
                else
                {
                    throw new Exception($"Failed to create Manager user: {string.Join("; ", result.Errors.Select(e => e.Description))}");
                }
            }

            // 4. Seed Admin UserIdentity without domain entity
            var adminEmail = "admin@mediappointment.com";
            var adminUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (adminUser == null)
            {
                adminUser = new UserIdentity
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Default Admin",
                    PhoneNumber = "0999888777",
                    EmailConfirmed = true,
                    RefreshToken = string.Empty,
                    RefreshTokenExpiryTime = DateTime.MinValue,
                    Status = Status.Active,
                    Gender = true,
                    DateOfBirth = new DateTime(1980, 7, 10)
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    throw new Exception($"Failed to create Admin user: {string.Join("; ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
