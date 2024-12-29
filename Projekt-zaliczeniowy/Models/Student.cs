namespace Projekt_zaliczeniowy.Models
{
    public class Student : User
    {
        public string SchoolLevel { get; set; }
        public int Grade { get; set; }
        public List<Lesson> Lessons { get; set; }
        public List<Review> ReviewsGiven { get; set; }
    }
}
