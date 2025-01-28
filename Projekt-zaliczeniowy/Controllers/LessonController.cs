using Microsoft.AspNetCore.Mvc;
using Projekt_zaliczeniowy.Models;

namespace Projekt_zaliczeniowy.Controllers
{
    public class LessonController : Controller
    {
        public IActionResult LessonMenu()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Create() // Tworzenie lekcji
        {
            var lesson = new Lesson
            {
                Id = 1,

            };
            return View();
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Details(int id) // Assuming Details requires an ID
        {
            // Fetch details using the ID
            var lesson = new Lesson { Id = id, Name = "Sample Lesson" }; // Replace with actual data fetching
            return View(lesson);
        }
    }
}
