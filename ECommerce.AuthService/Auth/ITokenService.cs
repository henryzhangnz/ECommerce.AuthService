using ECommerce.AuthService.Model;

namespace ECommerce.AuthService.Auth
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
