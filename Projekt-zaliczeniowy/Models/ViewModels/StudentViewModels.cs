using Projekt_zaliczeniowy.Models;

namespace Projekt_zaliczeniowy.Models.ViewModels
{
    public class StudentDashboardViewModel
    {
        public List<Lesson> UpcomingLessons { get; set; }
        public List<Lesson> RecentLessons { get; set; }
        public List<Payment> PendingPayments { get; set; }
    }
} 