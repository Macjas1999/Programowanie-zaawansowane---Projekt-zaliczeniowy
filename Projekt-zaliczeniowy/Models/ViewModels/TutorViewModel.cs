using Projekt_zaliczeniowy.Models;

namespace Projekt_zaliczeniowy.Models.ViewModels
{
    public class TutorViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Availability> Availabilities { get; set; }
    }
} 