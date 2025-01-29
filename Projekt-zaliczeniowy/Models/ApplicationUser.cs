using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Projekt_zaliczeniowy.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserType UserType { get; set; }
        public string? Description { get; set; }
        public decimal? HourlyRate { get; set; }

        // Navigation properties with explicit names to avoid ambiguity
        private ICollection<Subject> _teachingSubjects;
        public virtual ICollection<Subject> TeachingSubjects 
        {
            get => _teachingSubjects;
            private set => _teachingSubjects = value;
        }

        private ICollection<Lesson> _sessions;
        public virtual ICollection<Lesson> Sessions 
        {
            get => _sessions;
            private set => _sessions = value;
        }

        private ICollection<Availability> _availableSlots;
        public virtual ICollection<Availability> AvailableSlots 
        {
            get => _availableSlots;
            private set => _availableSlots = value;
        }

        private ICollection<Review> _receivedReviews;
        public virtual ICollection<Review> ReceivedReviews 
        {
            get => _receivedReviews;
            private set => _receivedReviews = value;
        }

        private ICollection<Review> _givenReviews;
        public virtual ICollection<Review> GivenReviews 
        {
            get => _givenReviews;
            private set => _givenReviews = value;
        }

        public ApplicationUser()
        {
            _teachingSubjects = new HashSet<Subject>();
            _sessions = new HashSet<Lesson>();
            _availableSlots = new HashSet<Availability>();
            _receivedReviews = new HashSet<Review>();
            _givenReviews = new HashSet<Review>();
        }
    }

    public enum UserType
    {
        Administrator,
        Korepetytor,
        Uzytkownik
    }
} 