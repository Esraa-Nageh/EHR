/*using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EHRsystem.Data;
using EHRsystem.Models.Entities;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace EHRsystem.Controllers{

public class MedicalRecordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MedicalRecordController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // === Upload File (GET) ===
        [HttpGet]
        public IActionResult UploadFile()
        {
            if (HttpContext.Session.GetString("UserRole") != "Doctor")
                return Unauthorized();

            return View();
        }

        // === Upload File (POST) ===
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string description, string fileType, int patientId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Doctor")
                return Unauthorized();

            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "No file selected.";
                return View();
            }

            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var fileName = Path.GetFileName(file.FileName);
            var path = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var doctorId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var record = new MedicalFile
            {
                FilePath = "/uploads/" + fileName,
                FileType = fileType,
                Description = description,
                PatientId = patientId,
                DoctorId = doctorId,
                UploadedAt = DateTime.Now
            };

            _context.MedicalFiles.Add(record);
            await _context.SaveChangesAsync();

            ViewBag.Message = "File uploaded successfully.";
            return View();
        }

        // === View Uploaded Files by Doctor ===
        public IActionResult ViewFiles()
        {
            if (HttpContext.Session.GetString("UserRole") != "Doctor")
                return Unauthorized();

            var doctorId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var files = _context.MedicalFiles
                .Where(f => f.DoctorId == doctorId)
                .ToList();

            return View(files);
        }
    }
}*/
