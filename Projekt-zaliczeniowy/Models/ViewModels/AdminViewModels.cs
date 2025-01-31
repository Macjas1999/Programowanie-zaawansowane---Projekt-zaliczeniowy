using System.ComponentModel.DataAnnotations;

namespace Projekt_zaliczeniowy.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public IList<ApplicationUser> TotalTeachers { get; set; }
        public IList<ApplicationUser> TotalStudents { get; set; }
        public List<Lesson> RecentLessons { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserType UserType { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane")]
        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Display(Name = "Obecna rola")]
        public string CurrentRole { get; set; }

        [Required(ErrorMessage = "Nowa rola jest wymagana")]
        [Display(Name = "Nowa rola")]
        public string NewRole { get; set; }
    }

    public class EditLessonViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        [Display(Name = "Nazwa lekcji")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana")]
        [Display(Name = "Data rozpoczęcia")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Data zakończenia jest wymagana")]
        [Display(Name = "Data zakończenia")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Status jest wymagany")]
        [Display(Name = "Status")]
        public LessonStatus Status { get; set; }

        [Required(ErrorMessage = "Cena jest wymagana")]
        [Display(Name = "Cena")]
        [Range(0, double.MaxValue, ErrorMessage = "Cena musi być większa od 0")]
        public decimal Price { get; set; }

        [Display(Name = "Notatki")]
        public string Notes { get; set; }

        [Required(ErrorMessage = "Korepetytor jest wymagany")]
        [Display(Name = "Korepetytor")]
        public string TeacherId { get; set; }

        [Required(ErrorMessage = "Uczeń jest wymagany")]
        [Display(Name = "Uczeń")]
        public string StudentId { get; set; }
    }

    public class EditPaymentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kwota jest wymagana")]
        [Display(Name = "Kwota")]
        [Range(0, double.MaxValue, ErrorMessage = "Kwota musi być większa od 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Status jest wymagany")]
        [Display(Name = "Status")]
        public PaymentStatus Status { get; set; }

        [Required(ErrorMessage = "Data płatności jest wymagana")]
        [Display(Name = "Data płatności")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Metoda płatności jest wymagana")]
        [Display(Name = "Metoda płatności")]
        public PaymentMethod Method { get; set; }

        public int LessonId { get; set; }
        
        [Display(Name = "Lekcja")]
        public string LessonName { get; set; }
        
        [Display(Name = "Nauczyciel")]
        public string TeacherName { get; set; }
        
        [Display(Name = "Uczeń")]
        public string StudentName { get; set; }
    }
} 