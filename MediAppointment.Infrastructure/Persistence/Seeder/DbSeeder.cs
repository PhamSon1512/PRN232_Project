namespace MediAppointment.Infrastructure.Persistence.Seeder
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            await RoleSeeder.SeedAsync(serviceProvider);
            await UserSeeder.SeedAsync(serviceProvider);
            // Thêm các seeder khác n?u c?n
        }
    }
}
