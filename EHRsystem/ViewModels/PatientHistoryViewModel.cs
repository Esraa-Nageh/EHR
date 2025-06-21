
using System.Collections.Generic;
using EHRsystem.Models.Entities; // For Patient, MedicalFile, Appointment, Prescription
using EHRsystem.Models; // For User (if Patient/Doctor inherit from it and it's in this namespace)

namespace EHRsystem.ViewModels
{
    public class PatientHistoryViewModel
    {
        public required Patient Patient { get; set; }
        public List<MedicalFile> MedicalFiles { get; set; } = new List<MedicalFile>();
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
        public List<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
