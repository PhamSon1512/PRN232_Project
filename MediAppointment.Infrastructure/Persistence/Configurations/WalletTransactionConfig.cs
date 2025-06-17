using MediAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediAppointment.Infrastructure.Persistence.Configurations
{
    public class WalletTransactionConfig : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.Type)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.Date)
                .IsRequired();

            builder.Property(t => t.Description)
                .HasMaxLength(255);
        }
    }
}
