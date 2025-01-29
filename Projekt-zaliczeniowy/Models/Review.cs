namespace Projekt_zaliczeniowy.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string TutorId { get; set; }
        public string StudentId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual ApplicationUser Tutor { get; set; }
        public virtual ApplicationUser Student { get; set; }
    }
}
