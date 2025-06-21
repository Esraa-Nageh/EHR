// File: EHRsystem/Models/Entities/Prescription.cs
// Last updated: Prescription entity model with new properties and navigation.

using System;
using System.Collections.Generic; // IMPORTANT: For ICollection
using System.ComponentModel.DataAnnotations;
using EHRsystem.Models.Entities; // IMPORTANT: If PharmacyRecommendation is in this namespace

namespace EHRsystem.Models.Entities
{
    public class Prescription
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [Required]
        public required string MedicationName { get; set; }
        [Required]
        public required string Dosage { get; set; }
        [Required]
        public required string Frequency { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        // NEW: Added this property for the controller logic
        public DateTime PrescriptionDate { get; set; }

        // REQUIRED: Add this navigation property for PharmacyRecommendation relationship
        public ICollection<PharmacyRecommendation>? PharmacyRecommendations { get; set; }
    }
}