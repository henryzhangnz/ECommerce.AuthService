using AutoFixture;
using ECommerce.AuthService.Model;
using ECommerce.AuthService.Repositories;

namespace Ecommerce.AuthService.Tests.RepositoryTests
{
    public class UserRepoTests
    {
        private readonly AppDbContextFixture _dbContextFixture;
        private readonly UserRepo _userRepo;

        public UserRepoTests()
        {
            _dbContextFixture = new AppDbContextFixture();
            _userRepo = new UserRepo(_dbContextFixture.AppDbContext);
        }

        [Fact]
        public async Task CheckIfUserExists_Should_ReturnTrueWhenUserExists()
        {
            // Arrange
            var fixture = new Fixture();
            var user = fixture.Create<User>();
            await _dbContextFixture.AppDbContext.Users.AddAsync(user);
            await _dbContextFixture.AppDbContext.SaveChangesAsync();

            // Action
            var result = await _userRepo.CheckIfUserExists(user.Username);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateUser_Should_CreateUserForValidUser()
        {
            // Arrange
            var fixture = new Fixture();
            var user = fixture.Create<User>();

            // Action
            await _userRepo.CreateUser(user);

            // Assert
            var createdUser = await _dbContextFixture.AppDbContext.Users.FindAsync(user.Id);
            Assert.NotNull(createdUser);
            Assert.Equal(user, createdUser);
        }

        [Fact]
        public async Task GetUser_Should_ReturnCorrectUser()
        {
            // Arrange
            var fixture = new Fixture();
            var user = fixture.Create<User>();

            // Action
            await _dbContextFixture.AppDbContext.Users.AddAsync(user);
            await _dbContextFixture.AppDbContext.SaveChangesAsync();
            var result = await _userRepo.GetUser(user.Username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user, result);
        }
    }
}
