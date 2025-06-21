using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EHRsystem.Data;
using EHRsystem.Models;
using EHRsystem.Models.Entities;
using EHRsystem.ViewModels;
using System; // Added for DateTime

namespace EHRsystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===== Register =====
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email exists
                var exists = _context.Users.Any(u => u.Email == model.Email);
                if (exists)
                {
                    ModelState.AddModelError("", "Email already registered.");
                    return View(model);
                }

                User user;

                // ✅ Use derived class based on selected role
                switch (model.Role)
                {
                    case "Doctor":
                        user = new Doctor
                        {
                            Name = model.Name,
                            Email = model.Email,
                            PasswordHash = HashPassword(model.Password),
                            Role = "Doctor",
                            Specialty = "", // Corrected: Using 'Specialty'
                            Location = ""
                        };
                        break;

                    case "Patient":
                        user = new Patient
                        {
                            Name = model.Name,
                            Email = model.Email,
                            PasswordHash = HashPassword(model.Password),
                            Role = "Patient",
                            NationalId = "",
                            Gender = "",
                            BirthDate = DateTime.Today
                        };
                        break;

                    case "Admin":
                        user = new AdminUser
                        {
                            Name = model.Name,
                            Email = model.Email,
                            PasswordHash = HashPassword(model.Password),
                            Role = "Admin"
                        };
                        break;

                    default:
                        ModelState.AddModelError("", "Invalid role selected.");
                        return View(model);
                }

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        // ===== Login =====
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.Name);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserEmail", user.Email);

                    return user.Role switch
                    {
                        "Doctor" => RedirectToAction("Dashboard", "Doctor"),
                        "Patient" => RedirectToAction("Dashboard", "Patient"),
                        "Admin" => RedirectToAction("ManageUsers", "Admin"),
                        _ => RedirectToAction("Index", "Home")
                    };
                }

                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(model);
        }

        // ===== Logout =====
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // ===== Hashing =====
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string entered, string storedHash)
        {
            return HashPassword(entered) == storedHash;
        }
    }

    // Optional: define AdminUser only if needed as a concrete type
    public class AdminUser : User { }
}