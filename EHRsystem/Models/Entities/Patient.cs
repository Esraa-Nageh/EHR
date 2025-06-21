// File: EHRsystem/Models/Entities/Patient.cs
// Last updated: Patient entity model with navigation properties.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EHRsystem.Models; // For the base User model
using System.Collections.Generic; // For ICollection

namespace EHRsystem.Models.Entities
{
    // Patient inherits from User (Table-Per-Hierarchy or TPH)
    public class Patient : User
    {
        // Assuming Name, Email, PasswordHash, Role come from User base class

        public string Gender { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; } = DateTime.Today;

        public string NationalId { get; set; } = string.Empty; // Or SSN, etc.

        // Navigation properties for relationships
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<MedicalFile> MedicalFiles { get; set; } = new List<MedicalFile>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
