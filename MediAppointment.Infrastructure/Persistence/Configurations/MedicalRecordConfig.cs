using MediAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediAppointment.Infrastructure.Persistence.Configurations
{
    public class MedicalRecordConfig : IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.BloodType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Property(m => m.Chronic)
                .HasMaxLength(250)
                .IsUnicode(true)
                .IsRequired(false);

            builder.Property(m => m.MedicalHistory)
                .IsUnicode(true)
                .IsRequired(false);

            builder.Property(m => m.MedicalResult)
                .IsUnicode(true)
                .IsRequired(false);

            builder.Property(m => m.LastUpdated)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(false);

            builder.HasOne<Doctor>()
                .WithMany()
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Patient>()
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
