using System.ComponentModel.DataAnnotations;

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

        // NEW: Added PrescriptionDate
        public DateTime PrescriptionDate { get; set; }

        public bool IsActive { get; set; }
    }
}