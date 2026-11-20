using Microsoft.EntityFrameworkCore;
using CourseAPI.Entities;

namespace CourseAPI.Data
{
    public class KursplassenDbContext : DbContext
    {
        public KursplassenDbContext(DbContextOptions<KursplassenDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Registration> Registrations => Set<Registration>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasColumnType("enum('Participant','Instructor')");

                // Unik e-post håndheves av databasen
                e.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<Course>(e =>
            {
                e.Property(c => c.Status)
                    .HasConversion<string>()
                    .HasColumnType("enum('Draft','Published','Cancelled')");

                // Ikke kaskadesletting fra bruker til kurs
                e.HasOne(c => c.Owner)
                    .WithMany()
                    .HasForeignKey(c => c.OwnerUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Registration>(e =>
            {
                e.HasOne(r => r.Course)
                    .WithMany()
                    .HasForeignKey(r => r.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.Participant)
                    .WithMany()
                    .HasForeignKey(r => r.ParticipantUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Maks én aktiv påmelding per deltaker per kurs
                e.HasIndex(r => new { r.CourseId, r.ActiveParticipantUserId }).IsUnique();
            });
        }
    }
}