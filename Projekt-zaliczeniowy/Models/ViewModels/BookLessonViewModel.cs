using System.ComponentModel.DataAnnotations;

namespace Projekt_zaliczeniowy.Models.ViewModels
{
    public class BookLessonViewModel
    {
        [Required]
        public int AvailabilityId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public string SubjectName { get; set; }

        public string? Notes { get; set; }
    }
} 