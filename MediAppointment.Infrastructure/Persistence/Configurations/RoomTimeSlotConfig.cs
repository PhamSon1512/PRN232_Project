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
    public class RoomTimeSlotConfig : IEntityTypeConfiguration<RoomTimeSlot>
    {
        public void Configure(EntityTypeBuilder<RoomTimeSlot> builder)
        {
            builder.HasKey(rts => rts.Id);

            builder.Property(rts => rts.Status)
                   .IsRequired();

            builder.HasOne(rts => rts.TimeSlot)
                   .WithMany(ts => ts.RoomSlots)
                   .HasForeignKey(rts => rts.TimeSlotId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(rts => rts.Room)
                   .WithMany(r => r.Departments)
                   .HasForeignKey(rts => rts.RoomId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(rts => rts.Doctor)
                   .WithMany() 
                   .HasForeignKey(rts => rts.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
