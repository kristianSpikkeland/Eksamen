using Microsoft.EntityFrameworkCore;
using CourseAPI.Entities;
using CourseAPI.Data.Repos.Interfaces;

namespace CourseAPI.Data.Repos
{
    public class RegistrationRepo(KursplassenDbContext db) : IRegistrationRepo
    {

        // Metoden er brukt som hjelpemetode for å slette kurs uten aktive påmeldinger. Kursrepo står for lagringen
        public async Task RemoveByCourseIdAsync(Guid courseId)
        {
            var regToDelete = await db.Registrations
                .Where(r => r.CourseId == courseId)
                .ToListAsync();

            db.Registrations.RemoveRange(regToDelete);
        }

        
        public async Task<RegistrationResult> CreateRegistrationAsync(Registration registration, DateTime nowUtc)
        {
            await using var tx = await db.Database.BeginTransactionAsync();

            var course = await db.Courses.FromSql($"SELECT * FROM courses WHERE id = {registration.CourseId} FOR UPDATE").SingleOrDefaultAsync();

            // Sjekker at kurset eksisterer
            if (course is null)
            {
                return RegistrationResult.NoCourse;
            }

            // Sjekker om kurset er publisert
            if (course.Status != CourseStatus.Published)
            {
                return RegistrationResult.NotPublished;
            }

            // Sjekker om kurset allerede er startet
            if (nowUtc >= course.StartsAtUtc)
            {
                return RegistrationResult.AlreadyStarted;
            }


            // Sjekker om deltaker har registrert seg tidligere
            if (await db.Registrations
                .AnyAsync(r => r.CourseId == course.Id && r.ParticipantUserId == registration.ParticipantUserId && r.CancelledAtUtc == null))
            {
                return RegistrationResult.AlreadyRegistered;
            }


            // Sjekker om kurset har plass til ny deltaker
            var regNumberByCourse = await db.Registrations.CountAsync(r => r.CourseId == course.Id && r.CancelledAtUtc == null);
            if (regNumberByCourse >= (int)course.Capacity)
            {
                return RegistrationResult.FullyBooked;
            }


            // Hvis all validering blir godkjent kan deltaker registreres
            registration.ActiveParticipantUserId = registration.ParticipantUserId;
            db.Registrations.Add(registration);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return RegistrationResult.Success;
        }


        public async Task<List<Registration>> GetRegistrationsWithCourseByUserAsync(Guid userId)
        {
            var regByUserId = await db.Registrations
                .AsNoTracking()
                .Include(r => r.Course)
                .Where(r => r.ParticipantUserId == userId)
                .OrderBy(r => r.RegisteredAtUtc)
                .ToListAsync();

            return regByUserId;
        }


        // Returnerer sporede objeket, beregnet for endring
        public async Task<Registration?> GetByIdAsync(Guid id) =>
            await db.Registrations.FirstOrDefaultAsync(r => r.Id == id);
        

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
    
}
