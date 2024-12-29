using Microsoft.AspNetCore.Mvc;

namespace Projekt_zaliczeniowy.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Process() // Przetwarzanie płatności
        {
            return View();
        }

        public IActionResult History() // Historia płatności
        {
            return View();
        }
    }
}
