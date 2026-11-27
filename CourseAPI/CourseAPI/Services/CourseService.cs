using CourseAPI.Data.Repos.Interfaces;
using CourseAPI.DTOs.Requests;
using CourseAPI.DTOs.Responses;
using CourseAPI.Entities;
using CourseAPI.Services.Validation;


namespace CourseAPI.Services
{
    public sealed class CourseService(ICourseRepo courseRepo, IRegistrationRepo registrationRepo, TimeProvider timeProvider, ILogger<CourseService> logger)
    {

        public static CourseDetailDTO ToDetailDTO(Course course, int availableSeats, int numberOfRegistrations) => new(
            course.Id,
            course.Title,
            course.Description,
            course.StartsAtUtc,
            course.EndsAtUtc,
            course.Location,
            course.Capacity,
            course.Status,
            course.UpdatedAtUtc,
            course.OwnerUserId,
            availableSeats,
            numberOfRegistrations
            );


        public static CourseSimpleDTO ToSimpleDTO(Course course) => new(
            course.Id,
            course.Title,
            course.StartsAtUtc,
            course.EndsAtUtc,
            course.Location,
            course.Capacity,
            course.Status
            );

      

        public async Task<IReadOnlyList<CourseSimpleDTO>> GetPublishedAndFutureCoursesAsync()
        {
            var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
            var courseDtoList = new List<CourseSimpleDTO>();

            var courses =  await courseRepo.GetPublishedAndFutureCoursesAsync(nowUtc);

            foreach (var course in courses)
            {
                courseDtoList.Add(ToSimpleDTO(course));
            }
            return courseDtoList;
        }


        // currentUserId er nullable siden anonyme brukere skal ha tilgang til å se publiserte kurs
        public async Task<CourseDetailDTO?> GetCourseDetailsAsync(Guid courseId, Guid? currentUserId)
        {

            var result = await courseRepo.GetWithActiveCountAsync(courseId);
            if (result is null) return null;            // kurset finnes ikke

            var course = result.Value.Course;

            // Hvis kurset ikke er publisert og instruktøren ikke eier kurs skal det ikke vises. Vi returnerer da 404 not found.
            if (course.OwnerUserId != currentUserId && course.Status != CourseStatus.Published)
            {
                return null;
            }

            var activeCount = result.Value.ActiveCount;

            int availableSeats = (int)course.Capacity - activeCount;

            return ToDetailDTO(course, availableSeats, activeCount);
        }


        public async Task<CourseSimpleDTO> CreateCourseAsync(CreateCourseRequest request, Guid ownerId)
        {
            RequestValidator.Validate(request);

            var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

            // Hvis all validering passerer kan vi kalle repository og mappe fra kurs entitet til DTO
            var course = await courseRepo.CreateCourseAsync(new Course
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                StartsAtUtc = request.StartsAtUtc!.Value,
                EndsAtUtc = request.EndsAtUtc!.Value,
                Location = request.Location.Trim(),
                Capacity = (uint)request.Capacity!.Value,
                Status = request.Status!.Value,
                CreatedAtUtc = nowUtc,
                UpdatedAtUtc = nowUtc,
                OwnerUserId = ownerId
            });

            logger.LogInformation("Kurs {CourseId} med tittel {CourseTitle} opprettet av {OwnerId}", course.Id, course.Title, ownerId);
            return ToSimpleDTO(course);
        }

        
        public async Task<(CourseSimpleDTO? CourseDto, UpdateStatus Status)> UpdateCourseAsync(UpdateCourseRequest request, Guid currentUserId, Guid courseId)
        {

            RequestValidator.Validate(request);

            var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

            var result = await courseRepo.GetWithActiveCountAsync(courseId);                  

            // Hvis kurset ikke finnes returneres ikke oppdatert kurs, kun enum med verdi CourseNotFound
            if (result is null)
            {
                return (null, UpdateStatus.CourseNotFound);
            }

            var course = result.Value.Course;

            // Hvis bruker ikke er eier av kurs, returneres ikke oppdatert kurs, kun enum med verdi NotOwner
            if (course.OwnerUserId != currentUserId)
            {
                return (null, UpdateStatus.NotOwner);
            }

            var activeCount = result.Value.ActiveCount;


            // Hvis bruker forsøker å endre slik at kapasiteten blir mindre enn antallet aktive påmeldinger
            // returneres ikke oppdatert kurs, kun enum med verdi CanNotChangeCapacity 
            if (request.Capacity < activeCount)
            {
                return (null, UpdateStatus.CanNotChangeCapacity);
            }       

            course.Title = request.Title.Trim();
            course.Description = request.Description.Trim();
            course.StartsAtUtc = request.StartsAtUtc!.Value;
            course.EndsAtUtc = request.EndsAtUtc!.Value;
            course.Location = request.Location.Trim();
            course.Capacity = (uint)request.Capacity!.Value;
            course.Status = request.Status!.Value;
            course.UpdatedAtUtc = nowUtc;

            await courseRepo.SaveChangesAsync();

            logger.LogInformation("Kurs {CourseId} med tittel {CourseTitle} endret av {OwnerId}", course.Id, course.Title, course.OwnerUserId);

            var dto = ToSimpleDTO(course);

            return (dto, UpdateStatus.Success);

        }

       
        public async Task<DeleteStatus> DeleteCourseWithoutActiveRegistrationsAsync(Guid courseId, Guid currentUserId)
        {
            var result = await courseRepo.GetWithActiveCountAsync(courseId);

            // Hvis kurset ikke finnes kan vi ikke slette det
            if (result is null)
            {
                return DeleteStatus.CourseNotFound;
            }

            var course = result.Value.Course;
            var activeCount = result.Value.ActiveCount;

            // Hvis bruker ikke er eier av kurs kan det ikke slettes av denne brukeren
            if (course.OwnerUserId != currentUserId)
            {
                return DeleteStatus.NotOwner;
            }

            // Hvis kurset har aktive registreringer kan det ikke slettes
            if (activeCount > 0)
            {
                return DeleteStatus.ActiveRegistrations;
            }

            // Hvis kurset består alle betingelser, kan det slettes

            // Men vi må først fjerne eventuelle gamle avbestillinger som ligger registrert
            await registrationRepo.RemoveByCourseIdAsync(courseId);

            await courseRepo.DeleteCourseAsync(course);


            logger.LogInformation("Kurs {CourseId} med tittel {CourseTitle} slettet av {OwnerId}", course.Id, course.Title, course.OwnerUserId);

            return DeleteStatus.Success;

        }
    }
}
