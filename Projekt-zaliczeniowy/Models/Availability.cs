namespace Projekt_zaliczeniowy.Models
{
    public class Availability
    {
        public int Id { get; set; }
        public Tutor Tutor { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
