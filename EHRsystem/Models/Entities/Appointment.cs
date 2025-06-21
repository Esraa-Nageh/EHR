using System;
using System.ComponentModel.DataAnnotations;

namespace EHRsystem.Models.Entities
{
    public class Appointment
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }

        [Required(ErrorMessage = "Appointment date is required.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime AppointmentDate { get; set; }

        public string Reason { get; set; } = string.Empty; // Already correctly initialized

        public string Status { get; set; } = "Pending";

        public bool IsBooked { get; set; } = false;
    }
}