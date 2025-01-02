using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Projekt_zaliczeniowy.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<TeacherController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Calendar(DateTime? date)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var selectedDate = date ?? DateTime.Today;
            var startOfWeek = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var lessons = await _context.Lessons
                .Include(l => l.Student)
                .Include(l => l.Name)
                .Where(l => l.TeacherId == currentUser.Id &&
                           l.StartTime >= startOfWeek &&
                           l.StartTime < endOfWeek)
                .OrderBy(l => l.StartTime)
                .ToListAsync();

            return View(lessons);
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> LessonDetails(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var lesson = await _context.Lessons
                .Include(l => l.Student)
                .Include(l => l.Name)
                .Include(l => l.Payment)
                .FirstOrDefaultAsync(l => l.Id == id && l.TeacherId == currentUser.Id);

            if (lesson == null)
            {
                return NotFound();
            }

            return PartialView("_LessonDetails", lesson);
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<IActionResult> AddAvailability(DateTime date, int hour)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var startTime = date.Date.AddHours(hour);
            var endTime = startTime.AddHours(1);

            var availability = new Availability
            {
                TutorId = currentUser.Id,
                StartTime = startTime,
                EndTime = endTime,
                IsBooked = false
            };

            _context.Availabilities.Add(availability);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<IActionResult> UpdateLessonStatus(int id, LessonStatus status)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Id == id && l.TeacherId == currentUser.Id);

            if (lesson == null)
            {
                return NotFound();
            }

            lesson.Status = status;
            if (status == LessonStatus.Completed)
            {
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.LessonId == lesson.Id);
                if (payment != null)
                {
                    payment.Status = PaymentStatus.Completed;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
