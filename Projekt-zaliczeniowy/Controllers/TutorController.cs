using Microsoft.AspNetCore.Mvc;

namespace Projekt_zaliczeniowy.Controllers
{
    public class TutorController : Controller
    {
        public IActionResult Index() // Lista wszystkich korepetytorów
        {
            return View();
        }

        public IActionResult Details(int id) // Szczegóły korepetytora
        {
            return View();
        }

        public IActionResult Schedule() // Harmonogram korepetytora
        {
            return View();
        }

        public IActionResult Availability() // Zarządzanie dostępnością
        {
            return View();
        }
    }
}
