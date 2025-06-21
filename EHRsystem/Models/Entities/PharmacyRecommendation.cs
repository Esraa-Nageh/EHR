using System.ComponentModel.DataAnnotations; // Add this if not already present

namespace EHRsystem.Models.Entities
{
    public class PharmacyRecommendation
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public Prescription? Prescription { get; set; } // <<-- CHANGED THIS LINE (made nullable)
        
        [Required]
        public required string PharmacyName { get; set; } // <<-- CHANGED THIS LINE
        [Required]
        public required string PharmacyLocation { get; set; } // <<-- CHANGED THIS LINE
    }
}