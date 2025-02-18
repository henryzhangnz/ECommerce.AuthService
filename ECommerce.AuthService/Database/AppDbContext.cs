using ECommerce.AuthService.Model;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.AuthService.Database
{
    public class AppDbContext(DbContextOptions opt): DbContext(opt)
    {
        public DbSet<User> Users { get; set; }
    }
}
