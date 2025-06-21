using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRsystem.Models.Entities
{
    public class MedicalFile
    {
        public int Id { get; set; }

        [Required]
        public required string Title { get; set; }

        public string? Description { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        // Optional file attachments (assuming these are not used as FilePath is used for single file)
        public string? PdfPath { get; set; }
        public string? ImagePath { get; set; }

        // === Uploader Info ===
        public int? PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        public int? DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }

        public string? FileType { get; set; }

        public string? FilePath { get; set; } // Correctly nullable

        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public string UploadedByRole { get; set; } = string.Empty; // "Doctor" or "Patient"
    }
}