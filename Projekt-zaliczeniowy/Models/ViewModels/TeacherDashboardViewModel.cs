using Projekt_zaliczeniowy.Models;

namespace Projekt_zaliczeniowy.Models.ViewModels
{
    public class TeacherDashboardViewModel
    {
        public List<Lesson> UpcomingLessons { get; set; }
        public List<Lesson> RecentLessons { get; set; }
        public List<Availability> Availabilities { get; set; }
    }
} 