using Microsoft.AspNetCore.Mvc;
using EHRsystem.Models;

namespace UnifiedEHRSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Automatically redirect to the Register page
            return RedirectToAction("Register", "Account");
        }
    }
}
