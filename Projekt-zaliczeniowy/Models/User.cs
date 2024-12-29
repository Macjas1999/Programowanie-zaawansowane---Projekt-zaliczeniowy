using Microsoft.AspNetCore.Identity;

namespace Projekt_zaliczeniowy.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserType UserType { get; set; }
        public string? Description { get; set; }
        public decimal? HourlyRate { get; set; }
        public List<Subject> TeachingSubjects { get; set; }
        public List<TutoringSession> Sessions { get; set; }
    }

    public enum UserType
    {
        Student,
        Teacher,
        Admin
    }
}
