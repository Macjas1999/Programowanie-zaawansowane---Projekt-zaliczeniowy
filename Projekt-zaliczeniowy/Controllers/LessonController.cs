using Microsoft.AspNetCore.Mvc;

namespace Projekt_zaliczeniowy.Controllers
{
    public class LessonController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        public IActionResult Create() // Tworzenie lekcji
        {
            return View();
        }

        public IActionResult Details(int id) // Szczegóły lekcji
        {
            return View();
        }

        public IActionResult Cancel(int id) // Anulowanie lekcji
        {
            return View();
        }
    }
}
