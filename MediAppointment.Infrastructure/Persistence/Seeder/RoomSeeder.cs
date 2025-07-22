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
    public static class RoomSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (!dbContext.Room.Any())
            {
                var rooms = new List<Room>();

                // Lấy tất cả Department từ database
                var departments = dbContext.Departments.ToList();

                int roomNumber = 101;

                foreach (var department in departments)
                {
                    // Mỗi Department có 2 phòng
                    rooms.Add(new Room
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Room {roomNumber++}",
                        DepartmentId = department.Id
                    });
                    rooms.Add(new Room
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Room {roomNumber++}",
                        DepartmentId = department.Id
                    });
                }

                dbContext.Room.AddRange(rooms);
                await dbContext.SaveChangesAsync();
                Console.WriteLine("✅ Seeded Rooms successfully.");
            }
            else
            {
                Console.WriteLine("ℹ️ Rooms already exist. Skipped seeding.");
            }
        }
    }
}
