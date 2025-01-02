using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Projekt_zaliczeniowy.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<StudentController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Calendar(DateTime? date)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var selectedDate = date ?? DateTime.Today;
            var startOfWeek = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var lessons = await _context.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Name)
                .Where(l => l.StudentId == currentUser.Id &&
                           l.StartTime >= startOfWeek &&
                           l.StartTime < endOfWeek)
                .OrderBy(l => l.StartTime)
                .ToListAsync();

            var availableSlots = await _context.Availabilities
                .Include(a => a.Tutor)
                .Where(a => !a.IsBooked &&
                           a.StartTime >= startOfWeek &&
                           a.StartTime < endOfWeek)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            ViewBag.AvailableSlots = availableSlots;
            return View(lessons);
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> LessonDetails(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var lesson = await _context.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Name)
                .Include(l => l.Payment)
                .FirstOrDefaultAsync(l => l.Id == id && l.StudentId == currentUser.Id);

            if (lesson == null)
            {
                return NotFound();
            }

            return PartialView("_LessonDetails", lesson);
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> GetTeacherSubjects(int availabilityId)
        {
            var availability = await _context.Availabilities
                .Include(a => a.Tutor)
                .ThenInclude(t => t.TeachingSubjects)
                .FirstOrDefaultAsync(a => a.Id == availabilityId);

            if (availability == null)
            {
                return NotFound();
            }

            var subjects = availability.Tutor.TeachingSubjects.Select(s => new
            {
                id = s.Id,
                name = s.Name
            });

            return Json(subjects);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> BookLesson(BookLessonViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var availability = await _context.Availabilities
                .Include(a => a.Tutor)
                .FirstOrDefaultAsync(a => a.Id == model.AvailabilityId && !a.IsBooked);

            if (availability == null)
            {
                return BadRequest("Ten termin nie jest już dostępny.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var lesson = new Lesson
                {
                    Id = model.SubjectId,
                    TeacherId = availability.TutorId,
                    StudentId = currentUser.Id,
                    StartTime = availability.StartTime,
                    EndTime = availability.EndTime,
                    Status = LessonStatus.Scheduled,
                    Notes = model.Notes,
                    Price = availability.Tutor.HourlyRate ?? 0
                };

                _context.Lessons.Add(lesson);
                availability.IsBooked = true;

                var payment = new Payment
                {
                    Lesson = lesson,
                    Amount = lesson.Price,
                    PaymentDate = DateTime.Now,
                    Status = PaymentStatus.Pending
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Błąd podczas rezerwacji lekcji");
                return BadRequest("Wystąpił błąd podczas rezerwacji lekcji.");
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> CancelLesson(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var lesson = await _context.Lessons
                .Include(l => l.Payment)
                .FirstOrDefaultAsync(l => l.Id == id &&
                                        l.StudentId == currentUser.Id &&
                                        l.Status == LessonStatus.Scheduled);

            if (lesson == null)
            {
                return NotFound();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                lesson.Status = LessonStatus.Cancelled;

                if (lesson.Payment != null)
                {
                    lesson.Payment.Status = PaymentStatus.Refunded;
                }

                var availability = await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.TutorId == lesson.TeacherId &&
                                            a.StartTime == lesson.StartTime);
                if (availability != null)
                {
                    availability.IsBooked = false;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Błąd podczas anulowania lekcji");
                return BadRequest("Wystąpił błąd podczas anulowania lekcji.");
            }
        }
    }

    // ViewModels/BookLessonViewModel.cs
    public class BookLessonViewModel
    {
        public int AvailabilityId { get; set; }
        public int SubjectId { get; set; }
        public string? Notes { get; set; }
    }
}
