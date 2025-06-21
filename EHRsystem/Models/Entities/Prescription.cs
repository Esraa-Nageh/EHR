using System.ComponentModel.DataAnnotations; // Add this if not already present

namespace EHRsystem.Models.Entities
{
    public class Prescription
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient? Patient { get; set; } // <<-- CHANGED THIS LINE (made nullable)
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; } // <<-- CHANGED THIS LINE (made nullable)
        
        [Required] // Add Required attribute for validation if not already present
        public required string MedicationName { get; set; } // <<-- CHANGED THIS LINE
        [Required]
        public required string Dosage { get; set; } // <<-- CHANGED THIS LINE
        [Required]
        public required string Frequency { get; set; } // <<-- CHANGED THIS LINE
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}