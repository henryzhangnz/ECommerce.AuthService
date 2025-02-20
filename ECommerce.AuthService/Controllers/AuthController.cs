using ECommerce.AuthService.Auth;
using ECommerce.AuthService.Model;
using ECommerce.AuthService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

            SetCookie(token);
            return Ok();
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

            string token = _tokenService.GenerateToken(user);
            SetCookie(token);
            return Ok();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(userId, out var guidId))
            {
                return BadRequest("Invalid UserId");
            }
            var user = await _userRepo.GetUserById(guidId);


            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
            };

            return Ok(userDto);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("authToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true in production (for HTTPS)
                SameSite = SameSiteMode.None,
            });
            return Ok(new { message = "Logged out successfully" });
        }

        private void SetCookie(string token)
        {
            // Store token in an HttpOnly cookie
            Response.Cookies.Append("authToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true in production (for HTTPS)
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });
        }
    }
}
