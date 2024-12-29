using Microsoft.AspNetCore.Mvc;

namespace Projekt_zaliczeniowy.Controllers
{
    public class StudentController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();

        //}
        public IActionResult Dashboard() // Panel ucznia
        {
            return View();
        }

        public IActionResult BookLesson() // Rezerwacja lekcji
        {
            return View();
        }

        public IActionResult MyLessons() // Historia lekcji
        {
            return View();
        }
    }
}
