using AutoFixture;
using ECommerce.AuthService.Auth;
using ECommerce.AuthService.Model;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Ecommerce.AuthService.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _tokenService = new TokenService(_configMock.Object);
        }

        [Fact]
        public void GenerateToken_Should_ReturnValidToken()
        {
            // Arrange
            var fixture = new Fixture();
            var user = fixture.Create<User>();

            _configMock.SetupGet(x => x["Jwt:Audience"]).Returns("audience");
            _configMock.SetupGet(x => x["Jwt:Issuer"]).Returns("Issuer");
            _configMock.SetupGet(x => x["Jwt:Key"]).Returns(fixture.CreateMany<char>(24).ToString());

            // Action
            var token = _tokenService.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
        }
    }
}
