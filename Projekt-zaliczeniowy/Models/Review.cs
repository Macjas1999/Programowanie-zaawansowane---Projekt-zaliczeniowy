namespace Projekt_zaliczeniowy.Models
{
    public class Review
    {
        public int Id { get; set; }
        public ApplicationUser Tutor { get; set; }
        public ApplicationUser Student { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
