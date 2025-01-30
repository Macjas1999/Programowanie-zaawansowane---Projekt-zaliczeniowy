using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt_zaliczeniowy.Data;
using Projekt_zaliczeniowy.Models;

namespace Projekt_zaliczeniowy.Controllers
{
    public class LessonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LessonController> _logger;

        public LessonController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<LessonController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [Authorize(Roles = "Korepetytor")]
        public IActionResult Create()
        {
            // Get list of students for dropdown
            var students = _context.Users
                .Where(u => u.UserType == UserType.Uzytkownik)
                .Select(u => new { Id = u.Id, Name = u.UserName })
                .ToList();

            ViewBag.Students = students;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Korepetytor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lesson lesson)
        {
            try
            {
                // Get the current teacher
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return NotFound("Nie znaleziono użytkownika.");
                }

                // Get the selected student
                var student = await _context.Users.FindAsync(lesson.StudentId);
                if (student == null)
                {
                    ModelState.AddModelError("StudentId", "Nie znaleziono wybranego ucznia.");
                    var students = _context.Users
                        .Where(u => u.UserType == UserType.Uzytkownik)
                        .Select(u => new { Id = u.Id, Name = u.UserName })
                        .ToList();
                    ViewBag.Students = students;
                    return View(lesson);
                }

                // Set up the lesson
                lesson.TeacherId = currentUser.Id;
                lesson.Teacher = currentUser;
                lesson.Student = student;
                lesson.Status = LessonStatus.Scheduled;

                // Create and save the lesson first
                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                // Create the payment record
                var payment = new Payment
                {
                    LessonId = lesson.Id,
                    Lesson = lesson,
                    Amount = lesson.Price,
                    PaymentDate = DateTime.Now,
                    Status = PaymentStatus.Pending
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Update the lesson with the payment
                lesson.Payment = payment;
                _context.Update(lesson);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Lekcja została pomyślnie utworzona!";
                return RedirectToAction("Index", "Teacher");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia lekcji");
                ModelState.AddModelError("", "Wystąpił błąd podczas tworzenia lekcji. Spróbuj ponownie.");
            }

            // If we got this far, something failed, redisplay form
            var studentsList = _context.Users
                .Where(u => u.UserType == UserType.Uzytkownik)
                .Select(u => new { Id = u.Id, Name = u.UserName })
                .ToList();

            ViewBag.Students = studentsList;
            return View(lesson);
        }

        [Authorize(Roles = "Korepetytor,Uzytkownik")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var lesson = await _context.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            // Check if the current user is either the teacher or the student of this lesson
            if (lesson.TeacherId != currentUser.Id && lesson.StudentId != currentUser.Id)
            {
                return Forbid();
            }

            return View(lesson);
        }

        [HttpPost]
        [Authorize(Roles = "Korepetytor,Uzytkownik")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var lesson = await _context.Lessons
                .Include(l => l.Payment)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            // Check if the current user is either the teacher or the student of this lesson
            if (lesson.TeacherId != currentUser.Id && lesson.StudentId != currentUser.Id)
            {
                return Forbid();
            }

            if (lesson.Status != LessonStatus.Scheduled)
            {
                return BadRequest("Tylko zaplanowane lekcje mogą być anulowane.");
            }

            lesson.Status = LessonStatus.Cancelled;
            if (lesson.Payment != null)
            {
                lesson.Payment.Status = PaymentStatus.Refunded;
            }

            await _context.SaveChangesAsync();

            if (await _userManager.IsInRoleAsync(currentUser, "Korepetytor"))
            {
                return RedirectToAction("Index", "Teacher");
            }
            else
            {
                return RedirectToAction("Index", "Student");
            }
        }
    }
}
