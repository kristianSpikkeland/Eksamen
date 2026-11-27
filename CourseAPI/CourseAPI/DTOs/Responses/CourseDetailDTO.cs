using CourseAPI.Entities;

namespace CourseAPI.DTOs.Responses
{
    public sealed record CourseDetailDTO(
        Guid Id,
        string Title,
        string Description,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        string Location,
        uint Capacity,
        CourseStatus Status,
        DateTime UpdatedAtUtc,
        Guid OwnerUserId,
        int AvailableSeats,
        int NumberOfRegistrations
        );

}
