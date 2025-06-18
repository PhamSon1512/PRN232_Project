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
            builder.Property(rts => rts.Date)
                   .IsRequired()
                   .HasColumnType("date");
        }
    }
}
