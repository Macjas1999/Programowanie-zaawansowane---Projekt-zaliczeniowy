using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt_zaliczeniowy.Data;
using Projekt_zaliczeniowy.Models;
using Projekt_zaliczeniowy.Models.ViewModels;

namespace Projekt_zaliczeniowy.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalTeachers = await _userManager.GetUsersInRoleAsync("Korepetytor"),
                TotalStudents = await _userManager.GetUsersInRoleAsync("Uzytkownik"),
                RecentLessons = await _context.Lessons
                    .Include(l => l.Teacher)
                    .Include(l => l.Student)
                    .OrderByDescending(l => l.StartTime)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserType = u.UserType
                })
                .ToListAsync();

            foreach (var user in users)
            {
                var userObj = await _userManager.FindByIdAsync(user.Id);
                user.Roles = await _userManager.GetRolesAsync(userObj);
            }

            return View(users);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CurrentRole = roles.FirstOrDefault()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.Email = model.Email;
            user.UserName = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.NewRole);

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Użytkownik został zaktualizowany.";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Użytkownik został usunięty.";
            }
            else
            {
                TempData["Error"] = "Wystąpił błąd podczas usuwania użytkownika.";
            }

            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Lessons()
        {
            var lessons = await _context.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Student)
                .OrderByDescending(l => l.StartTime)
                .ToListAsync();

            return View(lessons);
        }

        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Student)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            var viewModel = new EditLessonViewModel
            {
                Id = lesson.Id,
                Name = lesson.Name,
                StartTime = lesson.StartTime,
                EndTime = lesson.EndTime,
                Status = lesson.Status,
                Price = lesson.Price,
                Notes = lesson.Notes,
                TeacherId = lesson.TeacherId,
                StudentId = lesson.StudentId
            };

            ViewBag.Teachers = await _userManager.GetUsersInRoleAsync("Korepetytor");
            ViewBag.Students = await _userManager.GetUsersInRoleAsync("Uzytkownik");

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditLesson(EditLessonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Teachers = await _userManager.GetUsersInRoleAsync("Korepetytor");
                ViewBag.Students = await _userManager.GetUsersInRoleAsync("Uzytkownik");
                return View(model);
            }

            var lesson = await _context.Lessons.FindAsync(model.Id);
            if (lesson == null)
            {
                return NotFound();
            }

            lesson.Name = model.Name;
            lesson.StartTime = model.StartTime;
            lesson.EndTime = model.EndTime;
            lesson.Status = model.Status;
            lesson.Price = model.Price;
            lesson.Notes = model.Notes;
            lesson.TeacherId = model.TeacherId;
            lesson.StudentId = model.StudentId;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Lekcja została zaktualizowana.";
            return RedirectToAction(nameof(Lessons));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Lekcja została usunięta.";
            return RedirectToAction(nameof(Lessons));
        }

        public async Task<IActionResult> Payments()
        {
            var payments = await _context.Payments
                .Include(p => p.Lesson)
                .ThenInclude(l => l.Teacher)
                .Include(p => p.Lesson)
                .ThenInclude(l => l.Student)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }

        public async Task<IActionResult> EditPayment(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Lesson)
                .ThenInclude(l => l.Teacher)
                .Include(p => p.Lesson)
                .ThenInclude(l => l.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            var viewModel = new EditPaymentViewModel
            {
                Id = payment.Id,
                Amount = payment.Amount,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                Method = payment.Method,
                LessonId = payment.LessonId,
                LessonName = payment.Lesson.Name,
                TeacherName = $"{payment.Lesson.Teacher.FirstName} {payment.Lesson.Teacher.LastName}",
                StudentName = $"{payment.Lesson.Student.FirstName} {payment.Lesson.Student.LastName}"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPayment(EditPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var payment = await _context.Payments.FindAsync(model.Id);
            if (payment == null)
            {
                return NotFound();
            }

            payment.Status = model.Status;
            payment.Amount = model.Amount;
            payment.PaymentDate = model.PaymentDate;
            payment.Method = model.Method;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Płatność została zaktualizowana.";
            return RedirectToAction(nameof(Payments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Płatność została usunięta.";
            return RedirectToAction(nameof(Payments));
        }
    }
} 