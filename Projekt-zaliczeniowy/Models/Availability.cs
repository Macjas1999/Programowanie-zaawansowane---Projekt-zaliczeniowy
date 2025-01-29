namespace Projekt_zaliczeniowy.Models
{
    public class Availability
    {
        public int Id { get; set; }
        public string TutorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsBooked { get; set; }

        public virtual ApplicationUser Tutor { get; set; }
    }
}
