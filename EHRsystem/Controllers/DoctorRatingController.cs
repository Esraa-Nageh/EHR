using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EHRsystem.Data;
using EHRsystem.Models.Entities;
using System.Linq;

namespace EHRsystem.Controllers
{
    public class DoctorRatingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorRatingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Rate(int appointmentId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient") return Unauthorized();

            var existingRating = _context.DoctorRatings.FirstOrDefault(r => r.AppointmentId == appointmentId);
            if (existingRating != null)
            {
                ViewBag.Message = "You have already rated this appointment.";
                return View("AlreadyRated");
            }

            ViewBag.AppointmentId = appointmentId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Rate(int appointmentId, int rating, string comment)
        {
            if (HttpContext.Session.GetString("UserRole") != "Patient") return Unauthorized();

            var existing = _context.DoctorRatings.FirstOrDefault(r => r.AppointmentId == appointmentId);
            if (existing != null)
            {
                ViewBag.Message = "You already submitted a rating.";
                return View("AlreadyRated");
            }

            var newRating = new DoctorRating
            {
                AppointmentId = appointmentId,
                Rating = rating,
                Comment = comment
            };

            _context.DoctorRatings.Add(newRating);
            _context.SaveChanges();

            return RedirectToAction("Dashboard", "Patient");
        }
    }
}
