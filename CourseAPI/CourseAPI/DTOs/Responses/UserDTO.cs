using CourseAPI.Entities;

namespace CourseAPI.DTOs.Responses
{
    public sealed record UserDTO(
        Guid Id,
        string Email,
        string DisplayName,
        UserRole Role
        );
}
