using CourseAPI.Entities; 

namespace CourseAPI.DTOs.Responses
{
    public sealed record RegistrationDTO(
        Guid Id,
        DateTime RegisteredAtUtc,
        DateTime? CancelledAtUtc,

        // Utvalgte kurs egenskaper er tatt med siden vi skal vise egne påmeldinger med kursinformasjon
        Guid CourseId,
        string CourseTitle,
        string CourseDescription,
        DateTime CourseStartsAtUtc,
        DateTime CourseEndsAtUtc,
        string CourseLocation
        ); 
}
