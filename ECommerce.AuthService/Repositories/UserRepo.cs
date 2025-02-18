using ECommerce.AuthService.Database;
using ECommerce.AuthService.Model;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.AuthService.Repositories
{
    public class UserRepo : IUserRepo
    {
        private readonly AppDbContext _context;

        public UserRepo(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<bool> CheckIfUserExists(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Username == username);
            return user != null;
        }

        public async Task<Guid> CreateUser(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }

        public async Task<User> GetUser(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Username == username);
        }
    }
}
