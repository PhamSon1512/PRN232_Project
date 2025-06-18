using MediAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediAppointment.Infrastructure.Persistence.Configurations
{
    public class AppointmentConfig : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Note)
                .HasMaxLength(500)
                .IsUnicode(true)
                .IsRequired(false);

            builder.Property(a => a.Status)
                .IsRequired();

            builder.Property(a => a.CreatedDate)
                .IsRequired();

            builder.Property(a => a.UpdatedDate)
                .IsRequired(false);

            builder.Property(a => a.AppointmentDate)
                .IsRequired();

            builder.HasOne<Patient>()
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
