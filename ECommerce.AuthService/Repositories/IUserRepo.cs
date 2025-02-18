using ECommerce.AuthService.Model;

namespace ECommerce.AuthService.Repositories
{
    public interface IUserRepo
    {
        Task<Guid> CreateUser(User user);
        Task<bool> CheckIfUserExists(string username);
        Task<User> GetUser(string username);
    }
}
