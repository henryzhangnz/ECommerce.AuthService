using ECommerce.AuthService.Database;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.AuthService.Tests
{
    public class AppDbContextFixture : IDisposable
    {
        public AppDbContext AppDbContext { get; private set; }
        public AppDbContextFixture()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("TestDb");
            AppDbContext = new AppDbContext(options.Options);
            AppDbContext.Database.EnsureDeleted();
            AppDbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {

            AppDbContext.Dispose();
        }
    }
}