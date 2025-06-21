using System;
using System.Collections.Generic;
using EHRsystem.Models;

namespace EHRsystem.Models.Entities
{
    public class Patient : User
    {
        // ❌ Removed: public int UserId

        public string NationalId { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<MedicalFile> MedicalFiles { get; set; } = new List<MedicalFile>();
    }
}
