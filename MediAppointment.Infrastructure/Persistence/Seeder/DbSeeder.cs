namespace MediAppointment.Infrastructure.Persistence.Seeder
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            await RoleSeeder.SeedAsync(serviceProvider);
            await UserSeeder.SeedAsync(serviceProvider);
            // Th�m c�c seeder kh�c n?u c?n
        }
    }
}
