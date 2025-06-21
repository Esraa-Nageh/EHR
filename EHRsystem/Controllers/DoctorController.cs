using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EHRsystem.Data;
using EHRsystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EHRsystem.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsDoctor()
        {
            return HttpContext.Session.GetString("UserRole") == "Doctor";
        }

        /*public IActionResult Dashboard()
        {
            if (!IsDoctor()) return Unauthorized();

            int doctorId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var doctor = _context.Doctors.FirstOrDefault(d => d.Id == doctorId);
            if (doctor == null) return NotFound("Doctor profile not found.");

            // Dashboard data
            ViewBag.DoctorName = doctor.Name;
            ViewBag.TodayAppointmentsCount = _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == DateTime.Today)
                .Count();

            ViewBag.MyPatientsCount = _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Select(a => a.PatientId)
                .Distinct()
                .Count();

            ViewBag.RecordsCount = _context.MedicalFiles
                .Where(f => f.DoctorId == doctorId)
                .Count();

            return View();
        }*/


        // public IActionResult Dashboard()
        // {
        //     if (!IsDoctor()) return Unauthorized();

        //     int doctorId = HttpContext.Session.GetInt32("UserId") ?? 0;
        //     var doctor = _context.Doctors.FirstOrDefault(d => d.Id == doctorId);
        //     if (doctor == null) return NotFound("Doctor profile not found.");

        //     // === Dashboard Data ===
        //     ViewBag.DoctorName = doctor.Name;

        //     ViewBag.TodayAppointmentsCount = _context.Appointments
        //         .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == DateTime.Today)
        //         .Count();

        //     ViewBag.MyPatientsCount = _context.Appointments
        //         .Where(a => a.DoctorId == doctorId)
        //         .Select(a => a.PatientId)
        //         .Distinct()
        //         .Count();

        //     // Doctor's own uploaded records
        //     ViewBag.RecordsCount = _context.MedicalFiles
        //         .Where(f => f.DoctorId == doctorId)
        //         .Count();

        //     // ✅ New: All medical file count (for dashboard badge)
        //     ViewBag.MedicalFileCount = _context.MedicalFiles.Count();
        //     //doctor reminder 
        //     var doctorReminders = _context.Appointments
        //         .Where(a => a.IsBooked &&
        //                     a.DoctorId == doctorId &&
        //                     a.AppointmentDate >= DateTime.Now &&
        //                     a.AppointmentDate <= DateTime.Now.AddHours(24))
        //         .Include(a => a.Patient)
        //         .ToList();

        //     ViewBag.Reminders = doctorReminders;

        //     return View();
        // }


        public IActionResult Dashboard()
        {
            if (!IsDoctor()) return Unauthorized();

            int doctorId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var doctor = _context.Doctors.FirstOrDefault(d => d.Id == doctorId);
            if (doctor == null) return NotFound("Doctor profile not found.");

            // === Dashboard Data ===
            ViewBag.DoctorName = doctor.Name;

            ViewBag.TodayAppointmentsCount = _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == DateTime.Today)
                .Count();

            ViewBag.MyPatientsCount = _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Select(a => a.PatientId)
                .Distinct()
                .Count();

            // Doctor's own uploaded records
            ViewBag.RecordsCount = _context.MedicalFiles
                .Where(f => f.DoctorId == doctorId)
                .Count();

            // ✅ New: All medical file count (for dashboard badge)
            ViewBag.MedicalFileCount = _context.MedicalFiles.Count();

            // ✅ Reminder: Upcoming appointments within 24 hours
            var upcomingSoon = _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.IsBooked &&
                            a.AppointmentDate >= DateTime.Now &&
                            a.AppointmentDate <= DateTime.Now.AddHours(24))
                .OrderBy(a => a.AppointmentDate)
                .Include(a => a.Patient)
                .FirstOrDefault();

            if (upcomingSoon != null)
            {
                string patientName = upcomingSoon.Patient?.Name ?? "a patient";
                ViewBag.ReminderMessage = $"⏰ Reminder: You have an appointment with {patientName} on {upcomingSoon.AppointmentDate:f}";
            }

            // ✅ New: Average Rating
           // var ratings = _context.Appointments
                //.Where(a => a.DoctorId == doctorId && a.Rating > 0)
                //.Select(a => a.Rating)
                //.ToList();


           // ViewBag.AverageRating = ratings.Any() ? ratings.Average() : 0;
            return View();
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            if (!IsDoctor()) return Unauthorized();

            int doctorId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var doctor = _context.Doctors.FirstOrDefault(d => d.Id == doctorId);

            if (doctor == null)
                return NotFound("Doctor not found.");

            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(Doctor updated, string? newPassword)
        {
            if (!IsDoctor()) return Unauthorized();

            var doctor = _context.Doctors.FirstOrDefault(d => d.Id == updated.Id);
            if (doctor == null)
                return NotFound("Doctor not found.");

            doctor.Name = updated.Name;
            doctor.Email = updated.Email;
            doctor.Specialization = updated.Specialization;
            doctor.Specialty = updated.Specialty;
            doctor.Location = updated.Location;

            if (!string.IsNullOrWhiteSpace(newPassword))
                doctor.PasswordHash = HashPassword(newPassword);

            _context.SaveChanges();
            HttpContext.Session.SetString("UserName", doctor.Name);

            return RedirectToAction("Dashboard");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }


        // add reminder fn for appointment 

        public List<Appointment> GetUpcomingRemindersForUser(int userId, string role)
        {
            DateTime now = DateTime.Now;
            DateTime next24Hours = now.AddHours(24);

            if (role == "Patient")
            {
                return _context.Appointments
                    .Where(a => a.IsBooked
                        && a.PatientId == userId
                        && a.AppointmentDate >= now
                        && a.AppointmentDate <= next24Hours)
                    .Include(a => a.Doctor)
                    .ToList();
            }
            else if (role == "Doctor")
            {
                return _context.Appointments
                    .Where(a => a.IsBooked
                        && a.DoctorId == userId
                        && a.AppointmentDate >= now
                        && a.AppointmentDate <= next24Hours)
                    .Include(a => a.Patient)
                    .ToList();
            }

            return new List<Appointment>();
        }
        //view patient for doctors
        public IActionResult ViewPatients()
        {
            if (!IsDoctor()) return Unauthorized();

            int doctorId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var appointmentPatients = _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.IsBooked)
                .Select(a => a.Patient)
                .Where(p => p != null);

            var medicalFilePatients = _context.MedicalFiles
                .Where(m => m.DoctorId == doctorId)
                .Select(m => m.Patient)
                .Where(p => p != null);

            var allPatients = appointmentPatients
                .Union(medicalFilePatients)
                .Distinct()
                .ToList();

            return View(allPatients);
        }


    }
}
