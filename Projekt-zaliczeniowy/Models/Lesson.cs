namespace Projekt_zaliczeniowy.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public Tutor Tutor { get; set; }
        public Student Student { get; set; }
        public Subject Subject { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } // planned, completed, cancelled
        public string Notes { get; set; }
        public string MeetingLink { get; set; }
        public decimal Price { get; set; }
        public Payment Payment { get; set; }
    }
}
