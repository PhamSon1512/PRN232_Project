using MediAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Infrastructure.Persistence.Configurations
{
    public class TimeSlotConfig : IEntityTypeConfiguration<TimeSlot>
    {
        public void Configure(EntityTypeBuilder<TimeSlot> builder)
        {
            builder.HasKey(ts => ts.Id);

            builder.Property(ts => ts.TimeStart)
                   .IsRequired();

            builder.Property(ts => ts.Duration)
                   .IsRequired();

            builder.HasMany(ts => ts.RoomSlots)
                   .WithOne(rts => rts.TimeSlot)
                   .HasForeignKey(rts => rts.TimeSlotId);
        }
    }
}
