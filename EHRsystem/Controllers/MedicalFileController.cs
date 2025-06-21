using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using EHRsystem.Data;
using EHRsystem.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            {
                throw new InvalidOperationException("User ID is missing from session.");
            }
            return userId.Value;
        }

        private string GetUserRole()
        {
            return HttpContext.Session.GetString("UserRole") ?? "";
        }

        // === List Medical Files (for Patient or Doctor) ===
        public IActionResult Index(string? patientName, int? patientId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            string role = GetUserRole();
            int userId = GetUserId();

            var files = _context.MedicalFiles
               .Include(f => f.Patient)
               .Include(f => f.Doctor)
               .AsQueryable();

            if (role == "Patient")
            {
                files = files.Where(f => f.PatientId == userId);
            }
            else if (role == "Doctor")
            {
                if (!string.IsNullOrEmpty(patientName))
                {
                    files = files.Where(f => f.Patient != null && EF.Functions.Like(f.Patient.Name, $"%{patientName}%"));
                }

                if (patientId.HasValue)
                {
                    files = files.Where(f => f.PatientId == patientId.Value);
                }
            }
            else if (role == "Admin")
            {
                // Admins can see all files
            }
            else
            {
                return Unauthorized();
            }

            ViewBag.PatientNameFilter = patientName;
            ViewBag.PatientIdFilter = patientId;

            return View(files.OrderByDescending(f => f.UploadedAt).ToList());
        }

        [HttpGet]
        public IActionResult Upload()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var userRole = GetUserRole();

            if (userRole == "Doctor")
            {
                ViewBag.Patients = _context.Patients.ToList();
            }

            return View("UploadFile");
        }

        // === View Single Medical File ===
        public IActionResult ViewFile(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var file = _context.MedicalFiles
                .Include(f => f.Patient)
                .Include(f => f.Doctor)
                .FirstOrDefault(f => f.Id == id);

            if (file == null)
                return NotFound();

            string role = GetUserRole();
            int userId = GetUserId();

            bool authorizedToView = false;
            if (role == "Patient" && file.PatientId == userId)
            {
                authorizedToView = true;
            }
            else if (role == "Doctor" && file.DoctorId == userId)
            {
                authorizedToView = true;
            }
            else if (role == "Doctor" && file.PatientId.HasValue && _context.Appointments.Any(a => a.DoctorId == userId && a.PatientId == file.PatientId.Value))
            {
                authorizedToView = true;
            }
            else if (role == "Admin")
            {
                authorizedToView = true;
            }

            if (!authorizedToView)
            {
                return Forbid();
            }

            return View("ViewFile", file);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, string title, string? description, int? patientId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("Title", "Title is required.");
            }
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select a valid file.");
            }

            var userRole = GetUserRole();

            if (userRole == "Doctor" && !patientId.HasValue)
            {
                ModelState.AddModelError("PatientId", "Please select a patient.");
            }

            if (userRole == "Doctor")
            {
                ViewBag.Patients = _context.Patients.ToList();
            }

            if (!ModelState.IsValid)
            {
                return View("UploadFile");
            }

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file!.FileName);
            string filePathOnServer = Path.Combine(uploadsFolder, fileName);
            string filePathForDb = "/uploads/" + fileName;

            using (var stream = new FileStream(filePathOnServer, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            int actualPatientId = (userRole == "Doctor" && patientId.HasValue) ? patientId.Value : GetUserId();

            var medicalFile = new MedicalFile
            {
                Title = title,
                Description = description,
                FilePath = filePathForDb,
                FileType = file.ContentType,
                UploadedAt = DateTime.Now,
                UploadDate = DateTime.Now,
                UploadedByRole = userRole,
                PatientId = actualPatientId,
                DoctorId = (userRole == "Doctor") ? GetUserId() : null
            };

            _context.MedicalFiles.Add(medicalFile);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medical file uploaded successfully!";
            return RedirectToAction("Index");
        }

        // === Delete Medical File ===
        public IActionResult DeleteFile(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var file = _context.MedicalFiles.FirstOrDefault(f => f.Id == id);

            if (file == null)
                return NotFound();

            string role = GetUserRole();
            int userId = GetUserId();

            bool authorizedToDelete = false;
            if (role == "Patient" && file.PatientId == userId)
            {
                authorizedToDelete = true;
            }
            else if (role == "Doctor" && file.DoctorId == userId)
            {
                authorizedToDelete = true;
            }
            else if (role == "Admin")
            {
                authorizedToDelete = true;
            }

            if (!authorizedToDelete)
            {
                return Forbid();
            }

            string fullPath = Path.Combine(_environment.WebRootPath, file.FilePath?.TrimStart('/') ?? "");


            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.MedicalFiles.Remove(file);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Medical file deleted successfully!";
            return RedirectToAction("Index");
        }

        // === Download Medical File ===
        [HttpGet]
        public IActionResult Download(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var file = _context.MedicalFiles.FirstOrDefault(f => f.Id == id);

            if (file == null)
                return NotFound();

            string role = GetUserRole();
            int userId = GetUserId();

            bool authorizedToDownload = false;
            if (role == "Patient" && file.PatientId == userId)
            {
                authorizedToDownload = true;
            }
            else if (role == "Doctor" && file.DoctorId == userId)
            {
                authorizedToDownload = true;
            }
            else if (role == "Doctor" && file.PatientId.HasValue && _context.Appointments.Any(a => a.DoctorId == userId && a.PatientId == file.PatientId.Value))
            {
                authorizedToDownload = true;
            }
            else if (role == "Admin")
            {
                authorizedToDownload = true;
            }

            if (!authorizedToDownload)
            {
                return Forbid();
            }

            string fullPath = Path.Combine(_environment.WebRootPath, file.FilePath?.TrimStart('/') ?? "");


            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("File not found on server.");
            }

            string contentType = file.FileType ?? "application/octet-stream";

            return PhysicalFile(fullPath, contentType, file.Title + Path.GetExtension(file.FilePath));
        }
    }
}