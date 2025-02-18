using Microsoft.EntityFrameworkCore;

namespace ECommerce.AuthService.Database
{
    public static class PrepDatabase
    {
        public static void PrePopulation(this IApplicationBuilder builder)
        {
            using (var serviceScope = builder.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>()!);
            }

        }

        public static void SeedData(AppDbContext context)
        {
            if (!context.Database.CanConnect())
            {
                Console.WriteLine("Database does not exist. Running migrations...");
                context.Database.Migrate();
            }
            else
            {
                Console.WriteLine("Database already exists. Skipping migrations.");
            }
        }
    }
}
