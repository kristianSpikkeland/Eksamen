using CourseAPI.Data.Repos.Interfaces;
using CourseAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseAPI.Data.Repos
{
    public class UserRepo(KursplassenDbContext db) : IUserRepo
    {
        public async Task<User?> GetUserByEmailAsync(string email) =>
            await db.Users.FirstOrDefaultAsync(u => u.Email == email);


        public async Task<User> CreateUserAsync(User user)
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email) =>
            await db.Users.AnyAsync(u => u.Email == email);

    }
}
