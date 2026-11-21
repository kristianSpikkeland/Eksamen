using CourseAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseAPI.Data.Repos
{
    public class CourseRepo(KursplassenDbContext db)
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
    }
}
