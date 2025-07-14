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
                // Morning slots (7:00 AM - 11:00 AM)
                var baseTimes1 = new List<TimeSpan>
                {
                    new TimeSpan(7, 0, 0),
                    new TimeSpan(7, 30, 0),
                    new TimeSpan(8, 0, 0),
                    new TimeSpan(8, 30, 0),
                    new TimeSpan(9, 0, 0),
                    new TimeSpan(9, 30, 0),
                    new TimeSpan(10, 0, 0),
                    new TimeSpan(10, 30, 0)
                };

                var timeSlots1 = baseTimes1.Select(time => new TimeSlot
                {
                    Id = Guid.NewGuid(),
                    TimeStart = time,
                    Duration = new TimeSpan(0, 30, 0),
                    Shift = false // Morning = false
                }).ToList();

                // Afternoon slots (1:00 PM - 5:00 PM)
                var baseTimes2 = new List<TimeSpan>
                {
                    new TimeSpan(13, 0, 0),
                    new TimeSpan(13, 30, 0),
                    new TimeSpan(14, 0, 0),
                    new TimeSpan(14, 30, 0),
                    new TimeSpan(15, 0, 0),
                    new TimeSpan(15, 30, 0),
                    new TimeSpan(16, 0, 0),
                    new TimeSpan(16, 30, 0),
                };

                var timeSlots2 = baseTimes2.Select(time => new TimeSlot
                {
                    Id = Guid.NewGuid(),
                    TimeStart = time,
                    Duration = new TimeSpan(0, 30, 0),
                    Shift = true // Afternoon = true
                }).ToList();

                dbContext.TimeSlot.AddRange(timeSlots1);
                dbContext.TimeSlot.AddRange(timeSlots2);
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
