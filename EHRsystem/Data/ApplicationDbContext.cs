// File: EHRsystem/Data/ApplicationDbContext.cs
// Last updated: Corrected OnModelCreating to match current entity properties and relationships.

using Microsoft.EntityFrameworkCore;
using EHRsystem.Models; // For User model
using EHRsystem.Models.Entities; // For other entity models (Doctor, Patient, Prescription, MedicalFile, Admin, PharmacyRecommendation)

namespace EHRsystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all your entities
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Admin> Admins { get; set; } = default!; // CORRECTED: Changed from AdminUser to Admin
        public DbSet<Doctor> Doctors { get; set; } = default!;
        public DbSet<Patient> Patients { get; set; } = default!;
        public DbSet<Appointment> Appointments { get; set; } = default!;
        public DbSet<MedicalFile> MedicalFiles { get; set; } = default!;
        public DbSet<Prescription> Prescriptions { get; set; } = default!;
        public DbSet<PharmacyRecommendation> PharmacyRecommendations { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPH (Table Per Hierarchy) for User and its derived classes
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("Role")
                .HasValue<User>("User")
                .HasValue<Admin>("Admin") // CORRECTED: Changed from AdminUser to Admin
                .HasValue<Doctor>("Doctor")
                .HasValue<Patient>("Patient");

            // Configure relationships with OnDelete(DeleteBehavior.Restrict) to prevent cascading deletes

            // Appointments
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicalFiles
            modelBuilder.Entity<MedicalFile>()
                .HasOne(mf => mf.Doctor)
                .WithMany() // Doctor may not have an explicit collection of MedicalFiles, depends on design
                .HasForeignKey(mf => mf.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalFile>()
                .HasOne(mf => mf.Patient)
                .WithMany(p => p.MedicalFiles)
                .HasForeignKey(mf => mf.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescriptions
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany(d => d.Prescriptions)
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany(p => p.Prescriptions)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // PharmacyRecommendations
            modelBuilder.Entity<PharmacyRecommendation>()
                .HasOne(pr => pr.Prescription)
                .WithMany(p => p.PharmacyRecommendations) // This requires Prescription to have the PharmacyRecommendations collection
                .HasForeignKey(pr => pr.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

