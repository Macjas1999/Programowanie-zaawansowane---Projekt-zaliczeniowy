using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt_zaliczeniowy.Data;
using Projekt_zaliczeniowy.Models;
using Projekt_zaliczeniowy.Models.ViewModels;

namespace Projekt_zaliczeniowy.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<PaymentController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [Authorize(Roles = "Uzytkownik,Korepetytor")]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var isStudent = await _userManager.IsInRoleAsync(currentUser, "Uzytkownik");
            var payments = await _context.Payments
                .Include(p => p.Lesson)
                .ThenInclude(l => l.Teacher)
                .Where(p => isStudent ? 
                    p.Lesson.StudentId == currentUser.Id : 
                    p.Lesson.TeacherId == currentUser.Id)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }

        [Authorize(Roles = "Uzytkownik")]
        public async Task<IActionResult> Process(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Lesson)
                .ThenInclude(l => l.Teacher)
                .FirstOrDefaultAsync(p => p.Id == id && p.Lesson.StudentId == currentUser.Id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        [Authorize(Roles = "Uzytkownik,Korepetytor")]
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var isStudent = await _userManager.IsInRoleAsync(currentUser, "Uzytkownik");
            var payment = await _context.Payments
                .Include(p => p.Lesson)
                .ThenInclude(l => l.Teacher)
                .FirstOrDefaultAsync(p => p.Id == id && 
                    (isStudent ? p.Lesson.StudentId == currentUser.Id : 
                               p.Lesson.TeacherId == currentUser.Id));

            if (payment == null)
            {
                return NotFound();
            }

            return PartialView("_PaymentDetails", payment);
        }

        [HttpPost]
        [Authorize(Roles = "Uzytkownik")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Lesson)
                .FirstOrDefaultAsync(p => p.Id == id && p.Lesson.StudentId == currentUser.Id);

            if (payment == null)
            {
                return NotFound();
            }

            try
            {
                payment.Status = PaymentStatus.Completed;
                payment.PaymentDate = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Płatność została zrealizowana pomyślnie.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas przetwarzania płatności");
                TempData["Error"] = "Wystąpił błąd podczas przetwarzania płatności.";
                return RedirectToAction(nameof(Process), new { id });
            }
        }
    }
}
