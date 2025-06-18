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
    public static class TimeSlotSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (!dbContext.TimeSlot.Any())
            {
                var baseTimes = new List<TimeSpan>
                {
                    new TimeSpan(7, 0, 0),
                    new TimeSpan(8, 0, 0),
                    new TimeSpan(9, 0, 0),
                    new TimeSpan(10, 0, 0),
                    new TimeSpan(13, 0, 0),
                    new TimeSpan(14, 0, 0),
                    new TimeSpan(15, 0, 0),
                    new TimeSpan(16, 0, 0),
                };

                var timeSlots = baseTimes.Select(time => new TimeSlot
                {
                    Id = Guid.NewGuid(),
                    TimeStart = time,
                    Duration = new TimeSpan(1, 0, 0) 
                }).ToList();

                dbContext.TimeSlot.AddRange(timeSlots);
                await dbContext.SaveChangesAsync();

                Console.WriteLine("TimeSlots seeded successfully.");
            }
            else
            {
                Console.WriteLine("TimeSlots already exist. Skipped seeding.");
            }
        }
    }
}
