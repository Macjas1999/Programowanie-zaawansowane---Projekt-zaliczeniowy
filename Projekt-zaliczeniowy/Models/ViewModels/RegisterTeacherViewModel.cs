using System.ComponentModel.DataAnnotations;

namespace Projekt_zaliczeniowy.Models.ViewModels
{
    public class RegisterTeacherViewModel : RegisterViewModel
    {
        [Required(ErrorMessage = "Opis jest wymagany dla konta nauczyciela")]
        [MinLength(50, ErrorMessage = "Opis musi mieć co najmniej 50 znaków")]
        [Display(Name = "Opis doświadczenia")]
        public string TeacherDescription { get; set; }

        [Required(ErrorMessage = "Wybierz co najmniej jeden przedmiot")]
        [Display(Name = "Przedmioty nauczania")]
        public List<int> TeachingSubjects { get; set; }

        [Required(ErrorMessage = "Podaj stawkę godzinową")]
        [Range(0, 1000, ErrorMessage = "Stawka godzinowa musi być między 0 a 1000")]
        [Display(Name = "Stawka godzinowa (PLN)")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Dostępne dni tygodnia")]
        public List<DayOfWeek> AvailableDays { get; set; }
    }
}
