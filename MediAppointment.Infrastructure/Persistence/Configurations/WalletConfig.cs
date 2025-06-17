using MediAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediAppointment.Infrastructure.Persistence.Configurations
{
    public class WalletConfig : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.HasKey(w => w.Id);

            builder.HasIndex(w => w.UserId).IsUnique(); // M?i user ch? có 1 ví

            builder.Property(w => w.Balance)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.HasMany(w => w.Transactions)
                .WithOne()
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
