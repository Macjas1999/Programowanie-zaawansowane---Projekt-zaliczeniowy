namespace Projekt_zaliczeniowy.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TeacherId { get; set; }
        public string StudentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Price { get; set; }
        public LessonStatus Status { get; set; }
        public string? Notes { get; set; }

        public virtual ApplicationUser Teacher { get; set; }
        public virtual ApplicationUser Student { get; set; }
        public virtual Payment Payment { get; set; }
    }

    public enum LessonStatus
    {
        Scheduled,
        Completed,
        Cancelled,
        InProgress
    }
}
