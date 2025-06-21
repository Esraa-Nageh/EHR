using EHRsystem.Models;
using System.Collections.Generic;

namespace EHRsystem.Models.Entities
{
    public class Doctor : User
    {
        // Removed 'Specialization' as it was causing the unknown column error
        public string Specialty { get; set; } = string.Empty; // Keeping this one
        public string Location { get; set; } = string.Empty;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}