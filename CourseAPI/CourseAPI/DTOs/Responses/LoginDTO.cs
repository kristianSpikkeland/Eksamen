namespace CourseAPI.DTOs.Responses
{
    public sealed record LoginDTO(
        string JwtToken,
        DateTime ExpiresAtUtc);

}
