using Microsoft.EntityFrameworkCore;
using EHRsystem.Models;
using EHRsystem.Models.Entities;

namespace EHRsystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalFile> MedicalFiles { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PharmacyRecommendation> PharmacyRecommendations { get; set; }

        public DbSet<DoctorRating> DoctorRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Appointment -> Doctor
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment -> Patient
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicalFile -> Patient
            modelBuilder.Entity<MedicalFile>()
                .HasOne(m => m.Patient)
                .WithMany(p => p.MedicalFiles)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicalFile -> Doctor
            modelBuilder.Entity<MedicalFile>()
                .HasOne(m => m.Doctor)
                .WithMany()
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription -> Patient
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany()
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription -> Doctor
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany(d => d.Prescriptions)
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // PharmacyRecommendation -> Prescription
            modelBuilder.Entity<PharmacyRecommendation>()
                .HasOne(r => r.Prescription)
                .WithMany()
                .HasForeignKey(r => r.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // TPH (Table-per-Hierarchy) inheritance
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("Role")
                .HasValue<Doctor>("Doctor")
                .HasValue<Patient>("Patient")
                .HasValue<User>("Admin");
        }
    }
}
