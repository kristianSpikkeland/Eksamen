using CourseAPI.Entities;

namespace CourseAPI.Data.Repos.Interfaces
{
    public interface ICourseRepo
    {
        Task<List<Course>> GetPublishedAndFutureCoursesAsync(DateTime nowUtc);


        // Returnerer en tupple med kurs og antall deltakere. Ledige plasser må regnes ut i service 
        Task<(Course Course, int ActiveCount)?> GetWithActiveCountAsync(Guid courseId);


        // Oppretter kurs. Innlogget eier må settes i service
        Task<Course> CreateCourseAsync(Course course);


        // Returnerer sporede objeket, beregnet for endring
        Task<Course?> GetByIdAsync(Guid id);

        Task SaveChangesAsync();


        // Logikk rundt at kun kurs uten aktive påmeldinger slettes legges i service
        Task DeleteCourseAsync(Course course);
  

    }
}
