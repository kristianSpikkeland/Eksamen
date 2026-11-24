using CourseAPI.Entities;

namespace CourseAPI.Data.Repos.Interfaces
{
    public interface IRegistrationRepo
    {
        // Metoden er brukt som hjelpemetode for å slette kurs uten aktive påmeldinger. Kursrepo står for lagringen
        public Task RemoveByCourseIdAsync(Guid courseId);


        Task<RegistrationResult> CreateRegistrationAsync(Registration registration, DateTime nowUtc);

        Task<List<Registration>> GetRegistrationsWithCourseByUserAsync(Guid userId);


        // Returnerer sporede objeket, beregnet for endring
        Task<Registration?> GetByIdAsync(Guid id);

        Task SaveChangesAsync();
    }
}
