using Projekt_zaliczeniowy.Models;

namespace Projekt_zaliczeniowy.Models.ViewModels
{
    public class TeacherDashboardViewModel
    {
        public IEnumerable<Lesson> UpcomingLessons { get; set; }
        public IEnumerable<Lesson> RecentLessons { get; set; }
        public IEnumerable<Availability> Availabilities { get; set; }
    }
} 