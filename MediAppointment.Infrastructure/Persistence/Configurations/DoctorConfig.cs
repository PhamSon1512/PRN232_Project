using MediAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediAppointment.Infrastructure.Persistence.Configurations
{
    public class DoctorConfig : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            //builder.HasKey(d => d.Id);

            builder.HasMany(d => d.DoctorDepartments)
                .WithOne(dd => dd.Doctor)
                .HasForeignKey(dd => dd.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.Appointments)
                .WithOne()
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(d => d.Schedules)
                .WithOne()
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.Notifications)
                .WithOne()
                .HasForeignKey(n => n.ToUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
