namespace Projekt_zaliczeniowy.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string TeacherId { get; set; }
        public string StudentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Price { get; set; }
        public SessionStatus Status { get; set; }
        public string? Notes { get; set; }

        public ApplicationUser Teacher { get; set; }
        public ApplicationUser Student { get; set; }
    }
}
