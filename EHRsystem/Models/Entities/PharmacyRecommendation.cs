// File: EHRsystem/Models/Entities/PharmacyRecommendation.cs
// Last updated: PharmacyRecommendation entity model.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRsystem.Models.Entities
{
    public class PharmacyRecommendation
    {
        public int Id { get; set; }

        [Required]
        public string RecommendationText { get; set; } = string.Empty;
        public DateTime RecommendationDate { get; set; } = DateTime.Now;

        // Foreign key to Prescription
        public int PrescriptionId { get; set; }
        [ForeignKey("PrescriptionId")]
        public Prescription? Prescription { get; set; }
    }
}