using MediAppointment.Domain.Entities;
using MediAppointment.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediAppointment.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserIdentity, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserIdentity> UserIdentities { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorDepartment> DoctorDepartments { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserIdentity
            modelBuilder.Entity<UserIdentity>()
                .Property(u => u.CCCD)
                .HasMaxLength(12);

            // Department
            modelBuilder.Entity<Department>()
                .HasKey(d => d.DepartmentID);

            modelBuilder.Entity<Department>()
                .HasOne<UserIdentity>()
                .WithMany()
                .HasForeignKey(d => d.UpdatedPersonId);

            // DoctorDepartment (junction table)
            modelBuilder.Entity<DoctorDepartment>()
                .HasKey(dd => new { dd.DoctorID, dd.DepartmentID });

            modelBuilder.Entity<DoctorDepartment>()
                .HasOne(dd => dd.Doctor)
                .WithMany(d => d.DoctorDepartments)
                .HasForeignKey(dd => dd.DoctorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorDepartment>()
                .HasOne(dd => dd.Department)
                .WithMany()
                .HasForeignKey(dd => dd.DepartmentID)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor
            modelBuilder.Entity<Doctor>()
                .HasKey(d => d.DoctorID);

            modelBuilder.Entity<Doctor>()
                .HasOne<UserIdentity>()
                .WithOne()
                .HasForeignKey<Doctor>(d => d.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Appointment
            modelBuilder.Entity<Appointment>()
                .HasKey(a => a.AppointmentID);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Department)
                .WithMany()
                .HasForeignKey(a => a.DepartmentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne<UserIdentity>()
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Schedule
            modelBuilder.Entity<Schedule>()
                .HasKey(s => s.ScheduleID);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Doctor)
                .WithMany(d => d.Schedules)
                .HasForeignKey(s => s.DoctorID)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicalRecords
            modelBuilder.Entity<MedicalRecord>()
                .HasKey(mr => mr.RecordID);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(mr => mr.Doctor)
                .WithMany(d => d.MedicalRecords)
                .HasForeignKey(mr => mr.DoctorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne<UserIdentity>()
                .WithMany(u => u.MedicalRecords)
                .HasForeignKey(mr => mr.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Notifications
            modelBuilder.Entity<Notification>()
                .HasKey(n => n.NotifyID);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Doctor)
                .WithMany(d => d.Notifications)
                .HasForeignKey(n => n.DoctorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne<UserIdentity>()
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}