namespace Projekt_zaliczeniowy.Models
{
    public class Tutor : User
    {
        public string Education { get; set; }
        public string Biography { get; set; }
        public decimal HourlyRate { get; set; }
        public List<Subject> Subjects { get; set; }
        public List<Availability> AvailableSlots { get; set; }
        public List<Lesson> Lessons { get; set; }
        public List<Review> Reviews { get; set; }
    }
}
