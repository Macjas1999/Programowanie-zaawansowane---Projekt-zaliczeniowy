namespace Projekt_zaliczeniowy.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<ApplicationUser> Teachers { get; set; }

        public Subject()
        {
            Teachers = new HashSet<ApplicationUser>();
        }
    }
}
