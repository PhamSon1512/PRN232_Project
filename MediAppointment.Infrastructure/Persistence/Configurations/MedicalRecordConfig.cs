using MediAppointment.Domain.Entities;
using MediAppointment.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediAppointment.Infrastructure.Persistence.Configurations
{
    public class MedicalRecordConfig : IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.HasKey(m => m.Id);

            // ========== CẤU HÌNH CÁC TRƯỜNG ==========
            builder.Property(m => m.BloodType)
                .HasMaxLength(10)
                .IsUnicode(false);

            builder.Property(m => m.Chronic)
                .HasMaxLength(250)
                .IsUnicode(true);

            builder.Property(m => m.MedicalHistory).IsUnicode(true);
            builder.Property(m => m.MedicalResult).IsUnicode(true);
            builder.Property(m => m.Diagnosis).IsUnicode(true);
            builder.Property(m => m.TreatmentPlan).IsUnicode(true);
            builder.Property(m => m.Allergies).IsUnicode(true);
            builder.Property(m => m.Medications).IsUnicode(true);
            builder.Property(m => m.Symptoms).IsUnicode(true);
            builder.Property(m => m.VitalSigns).IsUnicode(true);
            builder.Property(m => m.DepartmentVisited).IsUnicode(true);
            builder.Property(m => m.DoctorName).IsUnicode(true);

          
            builder.Property(m => m.LastUpdated)
                .IsRequired(false); 

            // ========== CẤU HÌNH KHÓA NGOẠI ==========
            builder.HasOne<UserIdentity>() // FK tới AspNetUsers (Identity)
                .WithMany()
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<UserIdentity>()
                .WithMany()
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
