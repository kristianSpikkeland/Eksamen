using CourseAPI.Entities;

namespace CourseAPI.Data.Repos.Interfaces
{
    public interface IUserRepo
    {
        Task<User?> GetUserByEmailAsync(string email);

        Task<User> CreateUserAsync(User user);

        Task<bool> EmailExistsAsync(string email);

    }
}
