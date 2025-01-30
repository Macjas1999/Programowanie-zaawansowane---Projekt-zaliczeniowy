using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt_zaliczeniowy.Data;
using Projekt_zaliczeniowy.Models;
using Projekt_zaliczeniowy.Models.ViewModels;

namespace Projekt_zaliczeniowy.Controllers
{
    [Authorize(Roles = "Korepetytor")]
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

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User) as ApplicationUser;
            if (currentUser == null)
            {
                return NotFound();
            }

            var viewModel = new TeacherDashboardViewModel
            {
                UpcomingLessons = await _context.Lessons
                    .Include(l => l.Student)
                    .Where(l => l.TeacherId == currentUser.Id && 
                               l.StartTime > DateTime.Now && 
                               l.Status == LessonStatus.Scheduled)
                    .OrderBy(l => l.StartTime)
                    .Take(5)
                    .ToListAsync(),

                RecentLessons = await _context.Lessons
                    .Include(l => l.Student)
                    .Where(l => l.TeacherId == currentUser.Id && 
                               l.StartTime <= DateTime.Now)
                    .OrderByDescending(l => l.StartTime)
                    .Take(5)
                    .ToListAsync(),

                Availabilities = await _context.Availabilities
                    .Where(a => a.TutorId == currentUser.Id &&
                               a.StartTime > DateTime.Now)
                    .OrderBy(a => a.StartTime)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Calendar(DateTime? date)
        {
            var currentUser = await _userManager.GetUserAsync(User) as ApplicationUser;
            if (currentUser == null)
            {
                return NotFound();
            }

            var selectedDate = date ?? DateTime.Today;
            var startOfWeek = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var lessons = await _context.Lessons
                .Include(l => l.Student)
                .Include(l => l.Payment)
                .Where(l => l.TeacherId == currentUser.Id &&
                           l.StartTime >= startOfWeek &&
                           l.StartTime < endOfWeek)
                .OrderBy(l => l.StartTime)
                .ToListAsync();

            var availabilities = await _context.Availabilities
                .Where(a => a.TutorId == currentUser.Id &&
                           a.StartTime >= startOfWeek &&
                           a.StartTime < endOfWeek)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            ViewBag.Availabilities = availabilities;
            ViewBag.SelectedDate = selectedDate;
            return View(lessons);
        }

        [HttpGet]
        public async Task<IActionResult> LessonDetails(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User) as ApplicationUser;
            if (currentUser == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .Include(l => l.Student)
                .Include(l => l.Payment)
                .FirstOrDefaultAsync(l => l.Id == id && l.TeacherId == currentUser.Id);

            if (lesson == null)
            {
                return NotFound();
            }

            return PartialView("_LessonDetails", lesson);
        }

        [HttpPost]
        public async Task<IActionResult> AddAvailability(DateTime date, int hour)
        {
            var currentUser = await _userManager.GetUserAsync(User) as ApplicationUser;
            if (currentUser == null)
            {
                return NotFound();
            }

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

        [HttpPost]
        public async Task<IActionResult> UpdateLessonStatus(int id, LessonStatus status)
        {
            var currentUser = await _userManager.GetUserAsync(User) as ApplicationUser;
            if (currentUser == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .Include(l => l.Payment)
                .FirstOrDefaultAsync(l => l.Id == id && l.TeacherId == currentUser.Id);

            if (lesson == null)
            {
                return NotFound();
            }

            lesson.Status = status;
            if (status == LessonStatus.Completed)
            {
                if (lesson.Payment != null)
                {
                    lesson.Payment.Status = PaymentStatus.Completed;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User) as ApplicationUser;
            if (currentUser == null)
            {
                return NotFound();
            }

            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.Id == id && 
                                        a.TutorId == currentUser.Id && 
                                        !a.IsBooked);

            if (availability == null)
            {
                return Json(new { success = false, message = "Nie znaleziono dostępności lub jest już zarezerwowana." });
            }

            try
            {
                _context.Availabilities.Remove(availability);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania dostępności");
                return Json(new { success = false, message = "Wystąpił błąd podczas usuwania dostępności." });
            }
        }
    }
}
