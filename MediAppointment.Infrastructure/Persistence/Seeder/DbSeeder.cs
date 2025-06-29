using MediAppointment.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;


namespace MediAppointment.Infrastructure.Persistence.Seeder
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {

            // Lấy DbContext
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            //  Xóa foreign key sai nếu tồn tại
            var dropWrongForeignKeySql =
            @"
                IF EXISTS (
                    SELECT * FROM sys.foreign_keys 
                    WHERE name = 'FK_MedicalRecords_User_PatientId'
                )
                BEGIN
                    ALTER TABLE MedicalRecords
                    DROP CONSTRAINT FK_MedicalRecords_User_PatientId;
                END
            ";
            await context.Database.ExecuteSqlRawAsync(dropWrongForeignKeySql);

            await RoleSeeder.SeedAsync(serviceProvider);
            await UserSeeder.SeedAsync(serviceProvider);
            await DepartmentSeeder.SeedAsync(serviceProvider);
            await DepartmentDoctorSeeder.SeedAsync(serviceProvider);
            await RoleSeeder.SeedAsync(serviceProvider);
            await TimeSlotSeeder.SeedAsync(serviceProvider);
            await RoomSeeder.SeedAsync(serviceProvider);

        }
    }
}
