using System.ComponentModel.DataAnnotations;

namespace Projekt_zaliczeniowy.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Pole Email jest wymagane")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Pole Hasło jest wymagane")]
        [StringLength(100, ErrorMessage = "{0} musi mieć co najmniej {2} znaków długości.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasła nie są identyczne.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Pole Imię jest wymagane")]
        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Pole Nazwisko jest wymagane")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Wybierz typ użytkownika")]
        [Display(Name = "Typ użytkownika")]
        public UserType UserType { get; set; }

        // Pola dodatkowe dla nauczyciela
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [Display(Name = "Stawka godzinowa")]
        [Range(0, 1000, ErrorMessage = "Stawka godzinowa musi być między 0 a 1000")]
        public decimal? HourlyRate { get; set; }

        [Display(Name = "Przedmioty nauczania")]
        public List<int>? SelectedSubjects { get; set; }
    }
}
