using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EHRsystem.Data;
using EHRsystem.Models.Entities;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EHRsystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("UserId") != null;
        }

        private int GetUserId()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                throw new InvalidOperationException("User ID is missing from session.");
            }
            return userId.Value;
        }

        private string GetUserRole()
        {
            return HttpContext.Session.GetString("UserRole") ?? "";
        }

        // === PATIENT BOOKING PAGE (Browse available slots) ===
        [HttpGet]
        public IActionResult Book()
        {
            if (GetUserRole() != "Patient")
                return Unauthorized();

            var availableAppointments = _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => !a.IsBooked && a.AppointmentDate >= DateTime.Now)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            return View(availableAppointments);
        }

        // === PATIENT: View My Booked Appointments ===
        [HttpGet]
        public IActionResult MyAppointments()
        {
            if (GetUserRole() != "Patient")
                return Unauthorized();

            int patientId = GetUserId();

            var myAppointments = _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId && a.IsBooked)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            return View(myAppointments);
        }

        // === DOCTOR MANAGE APPOINTMENTS PAGE ===
        [HttpGet]
        public IActionResult Manage()
        {
            if (GetUserRole() != "Doctor")
                return Unauthorized();

            int doctorId = GetUserId();

            var myAppointments = _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            return View(myAppointments);
        }

        // === DOCTOR: ADD NEW APPOINTMENT SLOT (GET) ===
        [HttpGet]
        public IActionResult Create()
        {
            if (GetUserRole() != "Doctor")
                return Unauthorized();

            return View();
        }

        // === DOCTOR: ADD NEW APPOINTMENT SLOT (POST) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Appointment appointment)
        {
            if (GetUserRole() != "Doctor")
                return Unauthorized();

            appointment.DoctorId = GetUserId();
            appointment.IsBooked = false;
            appointment.Status = "Available";

            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Invalid form data. Please check the appointment date and time.";
                return View(appointment);
            }

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Appointment slot created successfully!";
            return RedirectToAction("Manage");
        }

        // === DELETE SLOT (Doctor can delete only their own unbooked slots) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (GetUserRole() != "Doctor")
                return Unauthorized();

            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt == null)
                return NotFound();

            if (appt.DoctorId != GetUserId())
                return Forbid();

            if (appt.IsBooked)
            {
                TempData["ErrorMessage"] = "Cannot delete a booked appointment.";
                return RedirectToAction("Manage");
            }

            _context.Appointments.Remove(appt);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Appointment slot deleted successfully.";
            return RedirectToAction("Manage");
        }

        // === PATIENT: GET form to reschedule/edit their appointment ===
        [HttpGet]
        public IActionResult EditAppointment(int id)
        {
            if (GetUserRole() != "Patient")
                return Unauthorized();

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            if (appointment.PatientId != GetUserId())
                return Forbid();

            return View(appointment);
        }

        // === PATIENT: POST to save rescheduled/edited appointment ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAppointment(Appointment updatedAppointment)
        {
            if (GetUserRole() != "Patient")
                return Unauthorized();

            var original = _context.Appointments.FirstOrDefault(a => a.Id == updatedAppointment.Id);
            if (original == null)
                return NotFound();

            if (original.PatientId != GetUserId())
                return Forbid();

            original.AppointmentDate = updatedAppointment.AppointmentDate;
            original.Reason = updatedAppointment.Reason;
            original.Status = "Rescheduled";

            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Invalid form data. Please check the date and time.";
                return View(updatedAppointment);
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Appointment rescheduled successfully!";
            return RedirectToAction("MyAppointments");
        }

        // === PATIENT: Unbook their own appointment ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Unbook(int id)
        {
            if (GetUserRole() != "Patient")
                return Unauthorized();

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            if (appointment.PatientId != GetUserId())
                return Forbid();

            appointment.PatientId = null;
            appointment.IsBooked = false;
            appointment.Status = "Available";
            appointment.Reason = string.Empty; // Corrected to assign string.Empty

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Appointment unbooked successfully!";
            return RedirectToAction("MyAppointments");
        }
    }
}