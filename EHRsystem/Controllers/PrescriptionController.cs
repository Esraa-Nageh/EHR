using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EHRsystem.Data;
using EHRsystem.Models.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace EHRsystem.Controllers
{
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionController(ApplicationDbContext context)
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

        // === GET: List Prescriptions (for Patient or Doctor) ===
        [HttpGet]
        public IActionResult Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            string role = GetUserRole();
            int userId = GetUserId();

            IQueryable<Prescription> prescriptions = _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Patient);

            if (role == "Patient")
            {
                prescriptions = prescriptions.Where(p => p.PatientId == userId);
            }
            else if (role == "Doctor")
            {
                prescriptions = prescriptions.Where(p => p.DoctorId == userId);
            }
            else if (role == "Admin")
            {
                // Admins can see all prescriptions
            }
            else
            {
                return Unauthorized();
            }

            return View(prescriptions.OrderByDescending(p => p.PrescriptionDate).ToList());
        }

        // === GET: Add Prescription Form (Doctor only) ===
        [HttpGet]
        public IActionResult Add()
        {
            if (GetUserRole() != "Doctor")
                return Unauthorized();

            ViewBag.Patients = _context.Patients.ToList();
            return View();
        }

        // === POST: Add Prescription (Doctor only) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Prescription prescription)
        {
            if (GetUserRole() != "Doctor")
                return Unauthorized();

            prescription.DoctorId = GetUserId();
            prescription.IsActive = true;
            prescription.PrescriptionDate = DateTime.Now; // Set the prescription date

            if (!ModelState.IsValid)
            {
                ViewBag.Patients = _context.Patients.ToList();
                TempData["ErrorMessage"] = "Invalid prescription data. Please check the fields.";
                return View(prescription);
            }

            _context.Prescriptions.Add(prescription);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Prescription added successfully.";
            return RedirectToAction("Index");
        }
    }
}