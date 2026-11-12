using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Data;

public static class ApplicationInitializer
{
    public static async Task InitializeShopEaseAsync(this WebApplication app)
    {
        // Seed roles
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (context.Database.IsSqlite()) await context.Database.EnsureCreatedAsync();
            else await context.Database.MigrateAsync();
            await IdentityInitializer.SeedAsync(scope.ServiceProvider, app.Configuration);

            // Seed sample data
            DbInitializer.Seed(context);
        }
    }
}
