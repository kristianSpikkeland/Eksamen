using CourseAPI.Data.Repos.Interfaces;
using CourseAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseAPI.Data.Repos
{
    public class CourseRepo(KursplassenDbContext db) : ICourseRepo
    {
        public async Task<List<Course>> GetPublishedAndFutureCoursesAsync(DateTime nowUtc)
        {
            var publishedCourses = await db.Courses
                .AsNoTracking()
                .Where(c => c.StartsAtUtc > nowUtc && c.Status == CourseStatus.Published)
                .OrderBy(c => c.StartsAtUtc)
                .ToListAsync();
                
            return publishedCourses;
        }


        // Returnerer en tupple med kurs og antall deltakere. Ledige plasser må regnes ut i service 
        public async Task<(Course Course, int ActiveCount)?> GetWithActiveCountAsync(Guid courseId)
        {
            var result = await db.Courses
                .Where(c => c.Id == courseId)
                .Select(c => new
                {
                    Course = c,
                    ActiveCount = db.Registrations.Count(r => r.CourseId == c.Id && r.CancelledAtUtc == null)
                })
                .FirstOrDefaultAsync();

            return result is null ? null : (result.Course, result.ActiveCount);
        }

        // Oppretter kurs. Innlogget eier må settes i service
        public async Task<Course> CreateCourseAsync(Course course)
        {
            db.Courses.Add(course);
            await db.SaveChangesAsync();
            return course;
        }

        // Returnerer sporede objeket, beregnet for endring
        public async Task<Course?> GetByIdAsync(Guid id) =>
            await db.Courses.FirstOrDefaultAsync(c => c.Id == id);

        public Task SaveChangesAsync() =>
            db.SaveChangesAsync();
        

        // Logikk rundt at kun kurs uten aktive påmeldinger slettes legges i service
        public async Task DeleteCourseAsync(Course course)
        {
            db.Courses.Remove(course);
            await db.SaveChangesAsync();
        }
      
    }


}
