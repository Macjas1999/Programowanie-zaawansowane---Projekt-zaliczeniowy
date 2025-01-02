namespace Projekt_zaliczeniowy.Models
{
    public class Availability
    {
        public int Id { get; set; }
        public ApplicationUser Teacher { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
