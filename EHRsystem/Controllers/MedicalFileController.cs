using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using EHRsystem.Data;
using EHRsystem.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // Added for List<MedicalFile> type hint
using System.Threading.Tasks; // Added for async/await

namespace EHRsystem.Controllers
{
    public class MedicalFileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MedicalFileController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("UserId") != null;
        }

        private int GetUserId()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                throw new Exception("Invalid or missing UserId in session.");

            return userId.Value;
        }

        private string GetUserRole()
        {
            return HttpContext.Session.GetString("UserRole") ?? "";
        }

        // === List Medical Files ===
        public IActionResult Index(string? patientName, int? patientId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            string role = GetUserRole();
            int userId = GetUserId();

            var files = _context.MedicalFiles
               .Include(f => f.Patient) // ✅ This loads the related patient
               .AsQueryable();


            if (role == "Patient")
            {
                files = files.Where(f => f.PatientId == userId);
            }
            else if (role == "Doctor")
            {
                if (!string.IsNullOrEmpty(patientName))
                {
                    var matchedPatientIds = _context.Patients
                        .Where(p => p.Name.Contains(patientName))
                        .Select(p => p.Id)
                        .ToList();

                    files = files.Where(f => matchedPatientIds.Contains(f.PatientId ?? 0));
                }

                if (patientId.HasValue)
                {
                    files = files.Where(f => f.PatientId == patientId.Value);
                }
            }

            ViewBag.PatientNameFilter = patientName;
            ViewBag.PatientIdFilter = patientId;

            return View(files.ToList());
        }

        [HttpGet]
        public IActionResult Upload()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole == "Doctor")
            {
                var patients = _context.Patients.ToList();
                ViewBag.Patients = patients;
            }

            return View("UploadFile");
        }



        public IActionResult ViewFile(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var file = _context.MedicalFiles
                .Include(f => f.Patient)
                .FirstOrDefault(f => f.Id == id);

            if (file == null)
                return NotFound();

            return View("ViewFiles", new List<MedicalFile> { file! }); // Fixed: Added null-forgiving operator to suppress CS8601
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, string title, string? description, int? patientId)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "No file selected.");
                // Ensure ViewBag.Patients is set for Doctor role if returning to view
                var userRoleForView = HttpContext.Session.GetString("UserRole");
                if (userRoleForView == "Doctor")
                {
                    ViewBag.Patients = _context.Patients.ToList();
                }
                return View("UploadFile");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            // Determine actual patient ID based on role and provided patientId
            int actualPatientId = (userRole == "Doctor" && patientId.HasValue) ? patientId.Value : GetUserId();

            var medicalFile = new MedicalFile
            {
                Title = title, // No longer an error if MedicalFile.Title is 'required'
                Description = description,
                FilePath = "/uploads/" + fileName,
                FileType = file.ContentType,
                UploadedAt = DateTime.Now,
                UploadDate = DateTime.Now, // Ensure this is set explicitly
                UploadedByRole = userRole ?? string.Empty, // Fixed: Handle null for UploadedByRole
                PatientId = actualPatientId,
                DoctorId = (userRole == "Doctor") ? GetUserId() : null
            };

            _context.MedicalFiles.Add(medicalFile);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");

        }

        public IActionResult DeleteFile(int id)
        {
            var file = _context.MedicalFiles.FirstOrDefault(f => f.Id == id);

            if (file == null)
                return NotFound();

            // Build full file path (removes leading slash from FilePath)
            // Fixed: Ensure FilePath is not null before TrimStart, though it should not be if assigned in Upload
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath?.TrimStart('/') ?? "");

            // Delete the file from the file system
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            // Delete the record from the database
            _context.MedicalFiles.Remove(file);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}