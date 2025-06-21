using Microsoft.AspNetCore.Mvc;
using EHRsystem.Models;

namespace EHRsystem.Controllers
{
    public class PredictionController : Controller
    {
        [HttpGet]
        public IActionResult Predict()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Predict(int Age, float BMI, int BloodPressure)
        {
            // Example dummy prediction
            string condition = (BMI > 25 || BloodPressure > 130) ? "Hypertension" : "Diabetes";
            int result = (int)(BMI + BloodPressure / 2); // Just for mockup

            ViewBag.Condition = condition;
            ViewBag.Result = result;
            return View("PredictionResult");
        }
    }
}
