using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Projekt_zaliczeniowy.Models;
using Microsoft.AspNetCore.Identity;

namespace Projekt_zaliczeniowy.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tutor> Tutors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja dziedziczenia dla Tutor i Student
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.TeachingSubjects)
                .WithMany(s => s.Teachers);

            // Konfiguracja relacji dla Lesson
            modelBuilder.Entity<Lesson>()
                .HasOne(s => s.Teacher)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.TeacherId);

            modelBuilder.Entity<Lesson>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId);

            // Konfiguracja relacji dla Availability
            modelBuilder.Entity<Availability>()
                .HasOne(a => a.Tutor)
                .WithMany(t => t.AvailableSlots)
                .HasForeignKey("TutorId");

            // Konfiguracja relacji dla Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Lesson)
                .WithOne(l => l.Payment)
                .HasForeignKey<Payment>("LessonId");

            // Konfiguracja relacji dla Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Tutor)
                .WithMany(t => t.Reviews)
                .HasForeignKey("TutorId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Student)
                .WithMany(s => s.ReviewsGiven)
                .HasForeignKey("StudentId")
                .OnDelete(DeleteBehavior.Restrict);

            // Dodatkowe konfiguracje właściwości
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Tutor>()
                .Property(t => t.HourlyRate)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Lesson>()
                .Property(l => l.Price)
                .HasColumnType("decimal(18,2)");

            // Indeksy
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Lesson>()
                .HasIndex(l => l.StartTime);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.PaymentDate);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=DefaultConnection");
            }

#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
#endif
        }
    }
}
