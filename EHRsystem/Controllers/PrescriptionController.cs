using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EHRsystem.Data;
using EHRsystem.Models.Entities;

namespace EHRsystem.Controllers
{
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === GET: Add Prescription ===
        [HttpGet]
        public IActionResult Add()
        {
            if (HttpContext.Session.GetString("UserRole") != "Doctor")
                return Unauthorized();

            // Optional: send doctor ID or list of patients to view
            return View();
        }

        // === POST: Add Prescription ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Prescription prescription)
        {
            if (HttpContext.Session.GetString("UserRole") != "Doctor")
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid prescription data.";
                return View(prescription);
            }

            prescription.DoctorId = HttpContext.Session.GetInt32("UserId") ?? 0;
            prescription.IsActive = true;

            _context.Prescriptions.Add(prescription);
            _context.SaveChanges();

            ViewBag.Message = "Prescription added successfully.";
            return View();
        }
    }
}
