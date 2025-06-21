using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using EHRsystem.Data;
using EHRsystem.Models;
using EHRsystem.Models.Entities;
using System;
using Microsoft.EntityFrameworkCore;

namespace EHRsystem.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === Dashboard ===
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            int patientId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId);

            if (patient == null)
                return NotFound();

            ViewBag.PatientName = patient.Name;

            var upcomingAppointments = _context.Appointments
                .Where(a => a.PatientId == patientId && a.AppointmentDate >= DateTime.Now)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            ViewBag.UpcomingAppointmentsCount = upcomingAppointments.Count;
            ViewBag.NextAppointmentDate = upcomingAppointments.FirstOrDefault()?.AppointmentDate.ToString("MMM dd, yyyy @ h:mm tt") ?? "None";

            var medicalFiles = _context.MedicalFiles.Where(m => m.PatientId == patientId).ToList();
            ViewBag.MedicalRecordsCount = medicalFiles.Count;
            ViewBag.MedicalFileCount = medicalFiles.Count; // ✅ used in badge on dashboard button
            ViewBag.LastMedicalRecordDate = medicalFiles
                .OrderByDescending(m => m.UploadedAt)
                .Select(m => m.UploadedAt.ToString("MMM dd, yyyy"))
                .FirstOrDefault() ?? "N/A";

            ViewBag.PrescriptionsCount = _context.Prescriptions.Count(p => p.PatientId == patientId);
            ViewBag.ActivePrescriptionsCount = _context.Prescriptions.Count(p => p.PatientId == patientId && p.IsActive);
            ViewBag.MyDoctorsCount = _context.Appointments.Where(a => a.PatientId == patientId).Select(a => a.DoctorId).Distinct().Count();

            ViewBag.RecentMedicalRecords = medicalFiles
                .OrderByDescending(m => m.UploadedAt)
                .Take(2)
                .ToList();

            ViewBag.UpcomingAppointments = upcomingAppointments
            .Select(a =>
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.Id == a.DoctorId);
                return new
                {
                    Id = a.Id, // ✅ This fixes the runtime error
                    a.AppointmentDate,
                    DoctorName = doctor?.Name ?? "Doctor",
                    Specialization = doctor?.Specialization ?? "General"
                };
            }).ToList();
            //patient reminder 
            var reminders = _context.Appointments
                .Where(a => a.IsBooked &&
                            a.PatientId == patientId &&
                            a.AppointmentDate >= DateTime.Now &&
                            a.AppointmentDate <= DateTime.Now.AddHours(24))
                .Include(a => a.Doctor)
                .ToList();

            ViewBag.Reminders = reminders;
            // Step 1: Reminder logic - find appointments within next 24 hours
            var now = DateTime.Now;
            var next24h = now.AddHours(24);

            var upcomingSoon = _context.Appointments
                .Where(a => a.PatientId == patientId && a.IsBooked == true && a.AppointmentDate > now && a.AppointmentDate <= next24h)
                .OrderBy(a => a.AppointmentDate)
                .FirstOrDefault();

            if (upcomingSoon != null)
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.Id == upcomingSoon.DoctorId);
                string doctorName = doctor != null ? doctor.Name : "Your Doctor";

                ViewBag.ReminderMessage = $"⏰ Reminder: You have an appointment with Dr. {doctorName} on {upcomingSoon.AppointmentDate:f}";
            }

            return View();
        }

        // === Book Appointment (Search + Book Flow) ===
        [HttpGet]
        public IActionResult BookAppointment(string? specialty, string? location, string? name)
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            // Filter doctors who have available appointment slots
            var doctors = _context.Users.OfType<Doctor>()
                .Where(d => _context.Appointments.Any(a => a.DoctorId == d.Id && !a.IsBooked))
                .AsQueryable();

            if (!string.IsNullOrEmpty(specialty))
                doctors = doctors.Where(d => d.Specialty.Contains(specialty));

            if (!string.IsNullOrEmpty(location))
                doctors = doctors.Where(d => d.Location.Contains(location));

            if (!string.IsNullOrEmpty(name))
                doctors = doctors.Where(d => d.Name.Contains(name));

            var result = doctors
                .Select(d => new Doctor
                {
                    Id = d.Id,
                    Name = d.Name,
                    Specialty = d.Specialty,
                    Location = d.Location,
                    Appointments = _context.Appointments
                        .Where(a => a.DoctorId == d.Id && !a.IsBooked)
                        .OrderBy(a => a.AppointmentDate)
                        .ToList()
                })
                .ToList();

            ViewBag.Doctors = result;
            ViewBag.Specialties = _context.Users
                .OfType<Doctor>()
                .Select(d => d.Specialty)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        // public IActionResult BookAppointment(int appointmentId)
        // {
        //     if (HttpContext.Session.GetString("UserRole") != "Patient")
        //         return Unauthorized();

        //     var appointment = _context.Appointments.FirstOrDefault(a => a.Id == appointmentId && !a.IsBooked);
        //     if (appointment == null)
        //     {
        //         TempData["Error"] = "This appointment is no longer available.";
        //         return RedirectToAction("BookAppointment");
        //     }

        //     appointment.PatientId = HttpContext.Session.GetInt32("UserId") ?? 0;
        //     appointment.Status = "Confirmed";
        //     appointment.IsBooked = true;

        //     _context.SaveChanges();
        //     return RedirectToAction("Dashboard");
        // }

        public IActionResult BookAppointment(int appointmentId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == appointmentId && !a.IsBooked);
            if (appointment == null)
            {
                TempData["Error"] = "This appointment is no longer available.";
                return RedirectToAction("BookAppointment");
            }

            appointment.PatientId = HttpContext.Session.GetInt32("UserId") ?? 0;
            appointment.Status = "Confirmed";
            appointment.IsBooked = true;

            _context.SaveChanges();

            TempData["Success"] = "Appointment booked successfully!";
            return RedirectToAction("Dashboard", "Patient");
        }
        // === Edit Profile ===
        [HttpGet]
        public IActionResult EditProfile()
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            int? id = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(User updatedUser, string? NewPassword)
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (user == null)
                return NotFound();

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;

            if (!string.IsNullOrEmpty(NewPassword))
                user.PasswordHash = HashPassword(NewPassword);

            _context.SaveChanges();
            HttpContext.Session.SetString("UserName", user.Name);

            return RedirectToAction("Dashboard");
        }

        // === Medical Records ===
        public IActionResult MedicalRecords()
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            int patientId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var files = _context.MedicalFiles
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.UploadedAt)
                .ToList();

            return View(files);
        }

        [HttpGet]
        public IActionResult EditMedicalFile(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            var file = _context.MedicalFiles.FirstOrDefault(f => f.Id == id);
            if (file == null || file.PatientId != HttpContext.Session.GetInt32("UserId"))
                return Unauthorized();

            return View(file);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditMedicalFile(MedicalFile updated)
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            var file = _context.MedicalFiles.FirstOrDefault(f => f.Id == updated.Id);
            if (file == null || file.PatientId != HttpContext.Session.GetInt32("UserId"))
                return Unauthorized();

            file.FileType = updated.FileType;
            file.Description = updated.Description;

            _context.SaveChanges();
            return RedirectToAction("MedicalRecords");
        }

        [HttpGet]
        public IActionResult DeleteMedicalFile(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            var file = _context.MedicalFiles.FirstOrDefault(f => f.Id == id);
            if (file == null || file.PatientId != HttpContext.Session.GetInt32("UserId"))
                return Unauthorized();

            return View(file);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmDeleteMedicalFile(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            var file = _context.MedicalFiles.FirstOrDefault(f => f.Id == id);
            if (file == null || file.PatientId != HttpContext.Session.GetInt32("UserId"))
                return Unauthorized();

            _context.MedicalFiles.Remove(file);
            _context.SaveChanges();

            return RedirectToAction("MedicalRecords");
        }

        [HttpGet]
        public IActionResult UploadMedicalFile()
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMedicalFile(IFormFile file, string FileType, string Description)
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select a valid file.");
                return View();
            }

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var medicalFile = new MedicalFile
            {
                PatientId = HttpContext.Session.GetInt32("UserId") ?? 0,
                DoctorId = 0,
                FileType = FileType,
                Description = Description,
                FilePath = "/uploads/" + fileName,
                UploadedAt = DateTime.Now
            };

            _context.MedicalFiles.Add(medicalFile);
            _context.SaveChanges();

            return RedirectToAction("MedicalRecords");
        }

        // === Password Hashing Helper ===
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }


        // === View My Appointments ===
        public IActionResult MyAppointments()
        {
            int patientId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var appointments = _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Doctor) // optional: for doctor name
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return View(appointments);
        }

        // === Cancel Appointment ===
        [HttpPost]
        public IActionResult CancelAppointment(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null) return NotFound();

            // Make sure patient owns the appointment
            int patientId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (appointment.PatientId != patientId) return Unauthorized();

            appointment.Status = "Cancelled";
            _context.SaveChanges();

            return RedirectToAction("MyAppointments");
        }

        // === GET: Show form to reschedule ===
        [HttpGet]
        [HttpGet]
        public IActionResult EditAppointment(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null || appointment.PatientId != HttpContext.Session.GetInt32("UserId"))
                return NotFound();

            return View(appointment);
        }


        // === POST: Save rescheduled appointment ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAppointment(Appointment updatedAppointment)
        {
            int patientId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var original = _context.Appointments
                .FirstOrDefault(a => a.Id == updatedAppointment.Id && a.PatientId == patientId);

            if (original == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(updatedAppointment);
            }

            original.AppointmentDate = updatedAppointment.AppointmentDate;
            original.Reason = updatedAppointment.Reason;
            original.Status = "Rescheduled";

            _context.SaveChanges();
            return RedirectToAction("MyAppointments");
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


    }
}
