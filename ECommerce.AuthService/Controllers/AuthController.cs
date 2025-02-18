using ECommerce.AuthService.Auth;
using ECommerce.AuthService.Model;
using ECommerce.AuthService.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.AuthService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepo _userRepo;

        public AuthController(ITokenService tokenService, IUserRepo userRepo)
        {
            _tokenService = tokenService;
            _userRepo = userRepo;
        }

        [HttpPost("register")]
        public async Task<ActionResult<string>> Register([FromBody] UserRegisterDto userRegisterDto)
        {
            if (await _userRepo.CheckIfUserExists(userRegisterDto.Username))
            {
                return BadRequest("User already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Role = RoleType.User,
                Email = userRegisterDto.Email,
                Username = userRegisterDto.Username,
            };

            string PasswordHash = new PasswordHasher<User>().HashPassword(user, userRegisterDto.Password);
            user.PasswordHash = PasswordHash;

            await _userRepo.CreateUser(user);
            string token = _tokenService.GenerateToken(user);

            return Ok(token);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] UserLoginDto userLoginDto)
        {
            if (!await _userRepo.CheckIfUserExists(userLoginDto.Username))
            {
                return NotFound($"Cannot find the user with username: ${userLoginDto.Username}");
            }

            var user = await _userRepo.GetUser(userLoginDto.Username);
            if (user == null || new PasswordHasher<User>()
                                        .VerifyHashedPassword(user, 
                                                            user.PasswordHash, 
                                                            userLoginDto.Password) == 
                                            PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(_tokenService.GenerateToken(user));
        }
    }
}
