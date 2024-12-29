namespace Projekt_zaliczeniowy.Models
{
    public class Review
    {
        public int Id { get; set; }
        public Tutor Tutor { get; set; }
        public Student Student { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
