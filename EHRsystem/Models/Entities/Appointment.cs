using System;
using System.ComponentModel.DataAnnotations;

namespace EHRsystem.Models.Entities
{
    public class Appointment
    {
        public int Id { get; set; }

        //[Required(ErrorMessage = "Doctor ID is required.")]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }  // ✅ Navigation property for EF

        // [Required(ErrorMessage = "Patient ID is required.")]
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }  // ✅ Navigation property for EF

        // [Required(ErrorMessage = "Appointment date is required.")]
        // [DataType(DataType.DateTime)]
        // public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Appointment date is required.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime AppointmentDate { get; set; }


        // [Required(ErrorMessage = "Reason is required.")]
        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";
        // ✅ NEW FIELD to allow patient booking logic

        public bool IsBooked { get; set; } = false;
        



    }
}
