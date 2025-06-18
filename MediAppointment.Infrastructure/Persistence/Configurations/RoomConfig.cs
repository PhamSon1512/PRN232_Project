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
    public class RoomConfig : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(r => r.DepartmentId)
                   .IsRequired(); 
            builder.HasMany(r => r.RoomTimeSlots)
                   .WithOne(rt => rt.Room)
                   .HasForeignKey(rt => rt.RoomId);
        }
    }
}
