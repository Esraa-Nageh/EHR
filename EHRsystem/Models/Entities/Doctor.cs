using EHRsystem.Models;
using System.Collections.Generic;

namespace EHRsystem.Models.Entities
{
    public class Doctor : User
    {
        public string Specialization { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
       

    }
}
