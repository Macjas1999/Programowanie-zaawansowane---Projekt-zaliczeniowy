using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projekt_zaliczeniowy.Models;

namespace Projekt_zaliczeniowy.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja relacji many-to-many między User a Subject
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.TeachingSubjects)
                .WithMany(s => s.Teachers)
                .UsingEntity(j => j.ToTable("UserSubjects"));

            // Konfiguracja relacji dla Lesson
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Teacher)
                .WithMany(u => u.Sessions)
                .HasForeignKey(l => l.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Student)
                .WithMany()
                .HasForeignKey(l => l.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracja relacji dla Availability
            modelBuilder.Entity<Availability>()
                .HasOne(a => a.Tutor)
                .WithMany(t => t.AvailableSlots)
                .HasForeignKey(a => a.TutorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracja relacji dla Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Lesson)
                .WithOne(l => l.Payment)
                .HasForeignKey<Payment>(p => p.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracja relacji dla Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Tutor)
                .WithMany(t => t.ReceivedReviews)
                .HasForeignKey(r => r.TutorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Student)
                .WithMany(s => s.GivenReviews)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfiguracja typów decimal
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ApplicationUser>()
                .Property(t => t.HourlyRate)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Lesson>()
                .Property(l => l.Price)
                .HasColumnType("decimal(18,2)");

            // Indeksy
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Lesson>()
                .HasIndex(l => l.StartTime);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.PaymentDate);
        }
    }
}