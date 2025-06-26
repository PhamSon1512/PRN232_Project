using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediAppointment.Infrastructure.Persistence.Configurations
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            // Shadow property UserIdentityId
            builder.Property<Guid?>("UserIdentityId")
                .HasColumnType("uniqueidentifier")
                .HasColumnName("UserIdentityId")
                .IsRequired(false);

            // ⚠️ Cấu hình navigation với shadow FK
            builder.HasOne(typeof(MediAppointment.Infrastructure.Identity.UserIdentity))
                .WithMany() // Không cần navigation ngược
                .HasForeignKey("UserIdentityId") // Tên shadow property
                .HasPrincipalKey(nameof(MediAppointment.Infrastructure.Identity.UserIdentity.Id)) // FK tới Id
                .HasConstraintName("FK_Users_AspNetUsers_UserIdentityId")
                .OnDelete(DeleteBehavior.Cascade);


            // Cấu hình các thuộc tính chung của User
            builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
            builder.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(15);
            builder.Property(u => u.DateOfBirth).IsRequired();
            builder.Property(u => u.Gender).IsRequired();
            builder.Property(u => u.Status)
                .IsRequired()
                .HasColumnType("int")
                .HasConversion<int>()
                .HasDefaultValue(Status.Active);
        }
    }
}