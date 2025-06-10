using MediAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediAppointment.Infrastructure.Persistence.Configurations
{
    public class DoctorDepartmentConfig : IEntityTypeConfiguration<DoctorDepartment>
    {
        public void Configure(EntityTypeBuilder<DoctorDepartment> builder)
        {
            builder.HasKey(dd => new { dd.DoctorId, dd.DepartmentId });

            builder.HasOne(dd => dd.Doctor)
                .WithMany(d => d.DoctorDepartments)
                .HasForeignKey(dd => dd.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(dd => dd.Department)
                .WithMany(d => d.DoctorDepartments)
                .HasForeignKey(dd => dd.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
