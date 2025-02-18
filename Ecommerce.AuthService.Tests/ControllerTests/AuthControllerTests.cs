using AutoFixture;
using ECommerce.AuthService.Auth;
using ECommerce.AuthService.Controllers;
using ECommerce.AuthService.Model;
using ECommerce.AuthService.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Ecommerce.AuthService.Tests.ControllerTests
{
    public class AuthControllerTests
    {
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IUserRepo> _userRepoMock;
        private readonly AuthController _authController;
        public AuthControllerTests()
        {
            _tokenServiceMock = new Mock<ITokenService>();
            _userRepoMock = new Mock<IUserRepo>();
            _authController = new AuthController(_tokenServiceMock.Object, _userRepoMock.Object);
        }

        [Fact]
        public async Task Register_Should_ReturnBadRequest_When_UserAlreadyExists()
        {
            // Arrange
            var fixture = new Fixture();
            var userRegisterDto = fixture.Create<UserRegisterDto>();

            _userRepoMock.Setup(x => x.CheckIfUserExists(userRegisterDto.Username))
                         .ReturnsAsync(true);

            // Act
            var result = await _authController.Register(userRegisterDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("User already exists.", badRequestResult.Value);
            _userRepoMock.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Register_Should_ReturnToken_When_RegistrationIsSuccessful()
        {
            // Arrange
            var fixture = new Fixture();
            var userRegisterDto = fixture.Create<UserRegisterDto>();

            _userRepoMock.Setup(x => x.CheckIfUserExists(userRegisterDto.Username))
                         .ReturnsAsync(false);

            User createdUser = null;
            _userRepoMock.Setup(x => x.CreateUser(It.IsAny<User>()))
                         .Callback<User>(user =>
                         {
                             user.Id = Guid.NewGuid();
                             createdUser = user; 
                         })
                         .ReturnsAsync(() => createdUser.Id);

            _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
                             .Returns("mocked_token");

            // Act
            var result = await _authController.Register(userRegisterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("mocked_token", okResult.Value);

            _userRepoMock.Verify(x => x.CreateUser(It.Is<User>(u => u.Username == userRegisterDto.Username)), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateToken(It.Is<User>(u => u.Username == userRegisterDto.Username)), Times.Once);
        }

        [Fact]
        public async Task Register_Should_HashPassword_BeforeStoringUser()
        {
            // Arrange
            var fixture = new Fixture();
            var userRegisterDto = fixture.Create<UserRegisterDto>();

            _userRepoMock.Setup(x => x.CheckIfUserExists(userRegisterDto.Username))
                         .ReturnsAsync(false);

            User createdUser = null;
            _userRepoMock.Setup(x => x.CreateUser(It.IsAny<User>()))
                         .Callback<User>(user => 
                         {
                             user.Id = Guid.NewGuid();
                             createdUser = user;
                         })
                         .ReturnsAsync(() => createdUser.Id);

            _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
                             .Returns("mocked_token");

            var hasher = new PasswordHasher<User>();

            // Act
            var result = await _authController.Register(userRegisterDto);

            // Assert
            Assert.NotNull(createdUser);
            Assert.NotNull(createdUser.PasswordHash);
            Assert.NotEqual(userRegisterDto.Password, createdUser.PasswordHash);

            var verifyResult = hasher.VerifyHashedPassword(createdUser, createdUser.PasswordHash, userRegisterDto.Password);
            Assert.Equal(PasswordVerificationResult.Success, verifyResult);
        }
    }
}