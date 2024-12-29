namespace Projekt_zaliczeniowy.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Level { get; set; }
        public List<ApplicationUser> Teachers { get; set; }
    }
}
