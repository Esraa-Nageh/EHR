using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EHRsystem.Data;
using EHRsystem.Models;
using EHRsystem.Models.Entities;
using System;

namespace EHRsystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // === Admin Dashboard ===
        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return Unauthorized();
            return View();
        }

        // === Manage Users ===
        public IActionResult ManageUsers()
        {
            if (!IsAdmin()) return Unauthorized();
            var users = _context.Users.ToList();
            return View(users);
        }

        // === Create User (GET) ===
        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!IsAdmin()) return Unauthorized();
            return View();
        }

        // === Create User (POST) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(string Name, string Email, string Role, string Password)
        {
            if (!IsAdmin()) return Unauthorized();

            if (_context.Users.Any(u => u.Email == Email))
            {
                ModelState.AddModelError("", "Email already exists.");
                return View();
            }

            User user;

            switch (Role)
            {
                case "Doctor":
                    user = new Doctor
                    {
                        Name = Name,
                        Email = Email,
                        PasswordHash = HashPassword(Password),
                        Role = "Doctor",
                        Specialty = "",
                        Location = ""
                    };
                    break;

                case "Patient":
                    user = new Patient
                    {
                        Name = Name,
                        Email = Email,
                        PasswordHash = HashPassword(Password),
                        Role = "Patient",
                        NationalId = "",
                        Gender = "",
                        BirthDate = DateTime.Today
                    };
                    break;

                case "Admin":
                    user = new AdminUser
                    {
                        Name = Name,
                        Email = Email,
                        PasswordHash = HashPassword(Password),
                        Role = "Admin"
                    };
                    break;

                default:
                    ModelState.AddModelError("", "Invalid role specified.");
                    return View();
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"User {user.Name} ({user.Role}) created successfully!";
            return RedirectToAction("ManageUsers");
        }

        // === Edit User (GET) ===
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // === Edit User (POST) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User updatedUser, string? newPassword)
        {
            if (!IsAdmin()) return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (user == null) return NotFound();

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;

            if (user is Doctor doctor && updatedUser is Doctor updatedDoctor)
            {
                doctor.Specialty = updatedDoctor.Specialty;
                doctor.Location = updatedDoctor.Location;
            }
            else if (user is Patient patient && updatedUser is Patient updatedPatient)
            {
                patient.NationalId = updatedPatient.NationalId;
                patient.Gender = updatedPatient.Gender;
                patient.BirthDate = updatedPatient.BirthDate;
            }

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                user.PasswordHash = HashPassword(newPassword);
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = $"User {user.Name} updated successfully!";
            return RedirectToAction("ManageUsers");
        }

        // === Delete User (GET) ===
        [HttpGet]
        public IActionResult DeleteUser(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // === Delete User (POST) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmDeleteUser(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"User {user.Name} deleted successfully!";
            return RedirectToAction("ManageUsers");
        }

        // === Hashing ===
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}