using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projekt_zaliczeniowy.Models;

//Appuser
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserType UserType { get; set; }
    public string? Description { get; set; }
    public decimal? HourlyRate { get; set; }
    public virtual ICollection<Subject> TeachingSubjects { get; set; }
    public virtual ICollection<Lesson> Sessions { get; set; }
    public virtual ICollection<Availability> AvailableSlots { get; set; }
    public virtual ICollection<Review> ReceivedReviews { get; set; }
    public virtual ICollection<Review> GivenReviews { get; set; }

    public ApplicationUser()
    {
        TeachingSubjects = new HashSet<Subject>();
        Sessions = new HashSet<Lesson>();
        AvailableSlots = new HashSet<Availability>();
        ReceivedReviews = new HashSet<Review>();
        GivenReviews = new HashSet<Review>();
    }
}

// Subject
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

// Lesson
public class Lesson
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string TeacherId { get; set; }
    public string StudentId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Price { get; set; }
    public LessonStatus Status { get; set; }
    public string? Notes { get; set; }

    public virtual ApplicationUser Teacher { get; set; }
    public virtual ApplicationUser Student { get; set; }
    public virtual Payment Payment { get; set; }
}

public enum LessonStatus
{
    Scheduled,
    Completed,
    Cancelled,
    InProgress
}

// Availability
public class Availability
{
    public int Id { get; set; }
    public string TutorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBooked { get; set; }

    public virtual ApplicationUser Tutor { get; set; }
}

// Payment
public class Payment
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }

    public virtual Lesson Lesson { get; set; }
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}

// Review
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

// ApplicationDbContext
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