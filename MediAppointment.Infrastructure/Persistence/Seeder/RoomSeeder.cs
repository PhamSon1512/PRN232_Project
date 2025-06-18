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
                var rooms = new List<Room>
                {
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        Name = "Room 101",
                        DepartmentId = Guid.Parse("0A438320-EEFE-4DB7-9DCA-51A17AB9AEE5")
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        Name = "Room 102",
                        DepartmentId = Guid.Parse("0A438320-EEFE-4DB7-9DCA-51A17AB9AEE5")
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        Name = "Room 201",
                        DepartmentId = Guid.Parse("4D484610-3623-465D-82C2-55C0337A5A11")
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        Name = "Room 202",
                        DepartmentId = Guid.Parse("4D484610-3623-465D-82C2-55C0337A5A11")
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        Name = "Room 301",
                        DepartmentId = Guid.Parse("E053F9BF-3195-4A42-904A-E99A615BE701")
                    }
                };

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
