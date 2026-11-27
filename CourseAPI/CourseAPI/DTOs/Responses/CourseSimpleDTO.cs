using CourseAPI.Entities;

namespace CourseAPI.DTOs.Responses
{
    public sealed record CourseSimpleDTO(
        Guid Id,
        string Title,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        string Location,
        uint Capacity,
        CourseStatus Status
        );
}
