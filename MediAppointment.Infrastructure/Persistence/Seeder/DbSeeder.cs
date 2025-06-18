namespace MediAppointment.Infrastructure.Persistence.Seeder
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
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
