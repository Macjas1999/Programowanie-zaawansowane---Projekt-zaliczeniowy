using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt_zaliczeniowy.Data;
using Projekt_zaliczeniowy.Models;
using Projekt_zaliczeniowy.Models.ViewModels;
using System.Linq;

namespace Projekt_zaliczeniowy.Controllers
{
    [Authorize(Roles = "Uzytkownik")]
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

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User) as ApplicationUser;
            if (currentUser == null)
            {
                return NotFound();
            }

            var viewModel = new StudentDashboardViewModel
            {
                UpcomingLessons = await _context.Lessons
                    .Include(l => l.Teacher)
                    .Where(l => l.StudentId == currentUser.Id && 
                               l.StartTime > DateTime.Now && 
                               l.Status == LessonStatus.Scheduled)
                    .OrderBy(l => l.StartTime)
                    .Take(5)
                    .ToListAsync(),

                RecentLessons = await _context.Lessons
                    .Include(l => l.Teacher)
                    .Where(l => l.StudentId == currentUser.Id && 
                               l.StartTime <= DateTime.Now)
                    .OrderByDescending(l => l.StartTime)
                    .Take(5)
                    .ToListAsync(),

                PendingPayments = await _context.Payments
                    .Include(p => p.Lesson)
                    .ThenInclude(l => l.Teacher)
                    .Where(p => p.Lesson.StudentId == currentUser.Id && 
                               p.Status == PaymentStatus.Pending)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Sessions()
        {
            var currentUser = await _userManager.GetUserAsync(User) as ApplicationUser;
            if (currentUser == null)
            {
                return NotFound();
            }

            var lessons = await _context.Lessons
                .Include(l => l.Teacher)
                .Where(l => l.StudentId == currentUser.Id)
                .OrderByDescending(l => l.StartTime)
                .ToListAsync();

            return View(lessons);
        }

        [HttpGet]
        public IActionResult Payments()
        {
            return RedirectToAction("Index", "Payment");
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
                .Include(l => l.Teacher)
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

        [HttpGet]
        public async Task<IActionResult> LessonDetails(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User) as ApplicationUser;
            if (currentUser == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Payment)
                .FirstOrDefaultAsync(l => l.Id == id && l.StudentId == currentUser.Id);

            if (lesson == null)
            {
                return NotFound();
            }

            return PartialView("_LessonDetails", lesson);
        }

        [HttpGet]
        public async Task<IActionResult> GetTeacherSubjects(int availabilityId)
        {
            // Get the tutor ID from availability
            var tutorId = await _context.Availabilities
                .Where(a => a.Id == availabilityId)
                .Select(a => a.TutorId)
                .FirstOrDefaultAsync();

            if (tutorId == null)
            {
                return NotFound();
            }

            // Get subjects using a join query
            var subjects = await (from us in _context.Set<Subject>()
                                join ts in _context.Set<Subject>("UserSubjects")
                                on us.Id equals EF.Property<int>(ts, "SubjectsId")
                                where EF.Property<string>(ts, "TeachersId") == tutorId
                                select new
                                {
                                    id = us.Id,
                                    name = us.Name
                                })
                                .ToListAsync();

            return Json(subjects);
        }

        [HttpPost]
        public async Task<IActionResult> BookLesson(BookLessonViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User) as ApplicationUser;
            if (currentUser == null)
            {
                return NotFound();
            }

            // Get the availability and tutor's hourly rate in a single query
            var availabilityInfo = await _context.Availabilities
                .Where(a => a.Id == model.AvailabilityId && !a.IsBooked)
                .Select(a => new
                {
                    Availability = a,
                    TutorRate = _context.Users
                        .Where(u => u.Id == a.TutorId)
                        .Select(u => EF.Property<decimal?>(u, "HourlyRate"))
                        .FirstOrDefault() ?? 0m
                })
                .FirstOrDefaultAsync();

            if (availabilityInfo == null)
            {
                return BadRequest("Ten termin nie jest już dostępny.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var lesson = new Lesson
                {
                    Name = model.SubjectName,
                    TeacherId = availabilityInfo.Availability.TutorId,
                    StudentId = currentUser.Id,
                    StartTime = availabilityInfo.Availability.StartTime,
                    EndTime = availabilityInfo.Availability.EndTime,
                    Status = LessonStatus.Scheduled,
                    Notes = model.Notes,
                    Price = availabilityInfo.TutorRate
                };

                _context.Lessons.Add(lesson);
                availabilityInfo.Availability.IsBooked = true;

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

        [HttpPost]
        public async Task<IActionResult> CancelLesson(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User) as ApplicationUser;
            if (currentUser == null)
            {
                return NotFound();
            }

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

        public async Task<IActionResult> FindTutor()
        {
            var tutors = await _context.Users
                .Where(u => u.UserType == UserType.Korepetytor)
                .Select(t => new TutorViewModel
                {
                    Id = t.Id,
                    Name = $"{t.FirstName} {t.LastName}",
                    Email = t.Email,
                    Availabilities = _context.Availabilities
                        .Where(a => a.TutorId == t.Id && !a.IsBooked && a.StartTime > DateTime.Now)
                        .OrderBy(a => a.StartTime)
                        .ToList()
                })
                .ToListAsync();

            return View(tutors);
        }
    }

    // ViewModels/BookLessonViewModel.cs
    public class BookLessonViewModel
    {
        public int AvailabilityId { get; set; }
        public int SubjectId { get; set; }
        public string? Notes { get; set; }
        public string SubjectName { get; set; }
    }
}
