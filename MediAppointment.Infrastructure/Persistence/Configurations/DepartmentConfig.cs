using MediAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediAppointment.Infrastructure.Persistence.Configurations
{
    public class DepartmentConfig : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.DepartmentName)
                .HasMaxLength(100)
                .IsUnicode(true)
                .IsRequired(false);

            builder.HasMany(d => d.DoctorDepartments)
                .WithOne(dd => dd.Department)
                .HasForeignKey(dd => dd.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(d => d.Rooms)
                   .WithOne(r => r.Department)
                   .HasForeignKey(r => r.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
