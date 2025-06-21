using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using EHRsystem.Data;
using EHRsystem.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

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

        // private int GetUserId()
        // {
        //     string? userIdString = HttpContext.Session.GetString("UserId");

        //     if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        //     {
        //         // Optionally redirect to login or throw unauthorized
        //         throw new Exception("Invalid or missing UserId in session.");
        //     }

        //     return userId;
        // }


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

        // === Upload Medical File (POST) ===
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult Upload(MedicalFile model, IFormFile? pdfFile, IFormFile? imageFile)
        // {
        //     if (!IsLoggedIn())
        //         return RedirectToAction("Login", "Account");

        //     string uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "medicalfiles");
        //     Directory.CreateDirectory(uploadsDir);

        //     if (pdfFile != null && pdfFile.Length > 0)
        //     {
        //         string pdfName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);
        //         string pdfPath = Path.Combine(uploadsDir, pdfName);
        //         using (var stream = new FileStream(pdfPath, FileMode.Create))
        //         {
        //             pdfFile.CopyTo(stream);
        //         }
        //         model.PdfPath = "/uploads/medicalfiles/" + pdfName;
        //     }

        //     if (imageFile != null && imageFile.Length > 0)
        //     {
        //         string imageName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
        //         string imagePath = Path.Combine(uploadsDir, imageName);
        //         using (var stream = new FileStream(imagePath, FileMode.Create))
        //         {
        //             imageFile.CopyTo(stream);
        //         }
        //         model.ImagePath = "/uploads/medicalfiles/" + imageName;
        //     }

        //     model.UploadDate = DateTime.Now;
        //     model.UploadedByRole = GetUserRole();

        //     if (model.UploadedByRole == "Doctor")
        //         model.DoctorId = GetUserId();
        //     else if (model.UploadedByRole == "Patient")
        //         model.PatientId = GetUserId();

        //     _context.MedicalFiles.Add(model);
        //     _context.SaveChanges();

        //     return RedirectToAction("Index");
        // }

        // [HttpGet]
        // public IActionResult Upload()
        // {
        //     return View("UploadFile"); // Make sure it matches the .cshtml filename
        // }
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

            return View("ViewFiles", new List<MedicalFile> { file });
        }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Upload(IFormFile file, string title)
        // {
        //     if (file == null || file.Length == 0)
        //     {
        //         ModelState.AddModelError("", "No file selected.");
        //         return View("UploadFile");
        //     }

        //     var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        //     Directory.CreateDirectory(uploadsFolder);

        //     var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        //     var filePath = Path.Combine(uploadsFolder, fileName);

        //     using (var stream = new FileStream(filePath, FileMode.Create))
        //     {
        //         await file.CopyToAsync(stream);
        //     }

        //     var medicalFile = new MedicalFile
        //     {
        //         Title = title,
        //         FilePath = "/uploads/" + fileName,
        //         FileType = file.ContentType,
        //         UploadedAt = DateTime.Now,
        //         PatientId = GetUserId()
        //     };

        //     _context.MedicalFiles.Add(medicalFile);
        //     await _context.SaveChangesAsync();

        //     return RedirectToAction("ViewFiles"); // Or wherever you want to go after uploading
        // }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, string title, string? description, int? patientId)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "No file selected.");
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

            // var userRole = HttpContext.Session.GetString("UserRole");

            // var medicalFile = new MedicalFile
            // {
            //     Title = title,
            //     Description = description,
            //     FilePath = "/uploads/" + fileName,
            //     FileType = file.ContentType,
            //     UploadedAt = DateTime.Now,
            //     UploadDate = DateTime.Now,
            //     UploadedByRole = userRole,
            //     PatientId = GetUserId(),
            //     DoctorId = (userRole == "Doctor") ? GetUserId() : null
            // };
            var userRole = HttpContext.Session.GetString("UserRole");
            int actualPatientId = (userRole == "Doctor" && patientId.HasValue) ? patientId.Value : GetUserId();

            var medicalFile = new MedicalFile
            {
                Title = title,
                Description = description,
                FilePath = "/uploads/" + fileName,
                FileType = file.ContentType,
                UploadedAt = DateTime.Now,
                UploadDate = DateTime.Now,
                UploadedByRole = userRole,
                PatientId = actualPatientId,
                DoctorId = (userRole == "Doctor") ? GetUserId() : null
            };

            _context.MedicalFiles.Add(medicalFile);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");

        }

        // === Delete Medical File ===
        //     public IActionResult Delete(int id)
        //     {
        //         if (!IsLoggedIn())
        //             return RedirectToAction("Login", "Account");

        //         var file = _context.MedicalFiles.FirstOrDefault(f => f.Id == id);
        //         if (file == null)
        //             return NotFound();

        //         // Allow only owner to delete
        //         string role = GetUserRole();
        //         int userId = GetUserId();
        //         if ((role == "Doctor" && file.DoctorId != userId) ||
        //             (role == "Patient" && file.PatientId != userId))
        //         {
        //             return Unauthorized();
        //         }

        //         // Delete files from disk
        //         if (!string.IsNullOrEmpty(file.PdfPath))
        //         {
        //             var fullPdfPath = _environment.WebRootPath + file.PdfPath.Replace("/", "\\");
        //             if (System.IO.File.Exists(fullPdfPath))
        //                 System.IO.File.Delete(fullPdfPath);
        //         }

        //         if (!string.IsNullOrEmpty(file.ImagePath))
        //         {
        //             var fullImagePath = _environment.WebRootPath + file.ImagePath.Replace("/", "\\");
        //             if (System.IO.File.Exists(fullImagePath))
        //                 System.IO.File.Delete(fullImagePath);
        //         }

        //         _context.MedicalFiles.Remove(file);
        //         _context.SaveChanges();

        //         return RedirectToAction("Index");
        //     }
        // }

        public IActionResult DeleteFile(int id)
        {
            var file = _context.MedicalFiles.FirstOrDefault(f => f.Id == id);

            if (file == null)
                return NotFound();

            // Build full file path (removes leading slash from FilePath)
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));

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
