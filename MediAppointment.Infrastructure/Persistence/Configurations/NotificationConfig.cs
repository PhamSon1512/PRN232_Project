using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediAppointment.Infrastructure.Persistence.Configurations
{
    public class NotificationConfig : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Content)
                .IsRequired()
                .IsUnicode(true);

            builder.Property(n => n.Status)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(n => n.CreatedAt)
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(n => n.ToUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
