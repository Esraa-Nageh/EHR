
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EHRsystem.Data;
using EHRsystem.Models.Entities;
using System;
using System.Linq;

namespace EHRsystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === PATIENT BOOKING PAGE ===
        [HttpGet]
        public IActionResult Book()
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient")
                return Unauthorized();

            var availableAppointments = _context.Appointments
                .Where(a => !a.IsBooked && a.AppointmentDate >= DateTime.Now)
                .ToList();

            return View(availableAppointments);
        }


        [HttpGet]
        public IActionResult Manage()
        {
            if (HttpContext.Session.GetString("UserRole") != "Doctor")
                return Unauthorized();

            int? doctorId = HttpContext.Session.GetInt32("UserId");
            if (doctorId == null)
                return RedirectToAction("Login", "Account");

            var myAppointments = _context.Appointments
                .Where(a => a.DoctorId == doctorId.Value)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            return View(myAppointments);
        }



        // === DOCTOR MANAGE PAGE ===
        // [HttpGet]
        // public IActionResult Manage()
        // {
        //     if (HttpContext.Session.GetString("UserRole") != "Doctor")
        //         return Unauthorized();

        //     // int doctorId = int.Parse(HttpContext.Session.GetString("UserId"));
        //     var userIdStr = HttpContext.Session.GetString("UserId");

        //     if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int doctorId))
        //     {
        //         return RedirectToAction("Login", "Account");
        //     }

        //     var myAppointments = _context.Appointments
        //         .Where(a => a.DoctorId == doctorId)
        //         .OrderBy(a => a.AppointmentDate)
        //         .ToList();

        //     return View(myAppointments);
        // }

        // // === ADD SLOT (GET) ===
        // [HttpGet]
        // public IActionResult Create()
        // {
        //     if (HttpContext.Session.GetString("UserRole") != "Doctor")
        //         return Unauthorized();

        //     return View();
        // }

        // === ADD SLOT (POST) ===
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult Create(Appointment appointment)
        // {
        //     if (HttpContext.Session.GetString("UserRole") != "Doctor")
        //         return Unauthorized();

        //     appointment.DoctorId = int.Parse(HttpContext.Session.GetString("UserId"));
        //     appointment.IsBooked = false;
        //     appointment.Status = "Available";

        //     if (ModelState.IsValid)
        //     {
        //         _context.Appointments.Add(appointment);
        //         _context.SaveChanges();
        //         return RedirectToAction("Manage");
        //     }

        //     return View(appointment);
        // }

        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") != "Doctor")
                return Unauthorized();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Appointment appointment)
        {
            if (HttpContext.Session.GetString("UserRole") != "Doctor")
                return Unauthorized();

            int? doctorId = HttpContext.Session.GetInt32("UserId");
            if (doctorId == null)
                return RedirectToAction("Login", "Account");

            appointment.DoctorId = doctorId.Value;
            appointment.IsBooked = false;
            appointment.Status = "Available";

            // 🔍 DEBUG: Show all model state errors
            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    Console.WriteLine($"Field: {entry.Key} | Error: {error.ErrorMessage}");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Invalid form data. Please try again.";
                return View(appointment);
            }

            _context.Appointments.Add(appointment);
            _context.SaveChanges();
            return RedirectToAction("Manage");
        }


        // === DELETE SLOT (Doctor can delete only unbooked) ===
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt != null && !appt.IsBooked)
            {
                _context.Appointments.Remove(appt);
                _context.SaveChanges();
            }
            return RedirectToAction("Manage");
        }


        // === GET: Edit Appointment ===
        [HttpGet]
        public IActionResult EditAppointment(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            return View(appointment); // Make sure you create the view
        }

        // === POST: Save Appointment ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAppointment(Appointment updatedAppointment)
        {
            if (!ModelState.IsValid)
                return View(updatedAppointment);

            var existing = _context.Appointments.FirstOrDefault(a => a.Id == updatedAppointment.Id);
            if (existing == null)
                return NotFound();

            existing.AppointmentDate = updatedAppointment.AppointmentDate;
            _context.SaveChanges();

            return RedirectToAction("Dashboard", "Patient");
        }


        [HttpGet]
        public IActionResult Unbook(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            // Only the patient who booked can unbook
            int patientId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (appointment.PatientId != patientId)
                return Unauthorized();

            appointment.PatientId = null; // or 0 depending on your model
            appointment.IsBooked = false;

            _context.SaveChanges();
            return RedirectToAction("Dashboard", "Patient");
        }

    }
}
