using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Data;

public static class IdentityInitializer
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var createdRole = await roleManager.CreateAsync(new IdentityRole(role));
                if (!createdRole.Succeeded)
                    throw new InvalidOperationException("Role creation failed: " +
                        string.Join("; ", createdRole.Errors.Select(error => error.Description)));
            }
        }

        // Seed admin account
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var adminEmail = configuration["SeedAdmin:Email"];
        var adminPassword = configuration["SeedAdmin:Password"];
        if (!string.IsNullOrWhiteSpace(adminEmail))
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                if (string.IsNullOrWhiteSpace(adminPassword))
                    throw new InvalidOperationException("Set SeedAdmin:Password to create the configured administrator.");
                adminUser = new AppUser
                {
                    UserName = adminEmail, Email = adminEmail,
                    FullName = "Admin", EmailConfirmed = true
                };
                var created = await userManager.CreateAsync(adminUser, adminPassword);
                if (!created.Succeeded)
                    throw new InvalidOperationException("Administrator creation failed: " +
                        string.Join("; ", created.Errors.Select(e => e.Description)));
            }
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var assigned = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (!assigned.Succeeded)
                    throw new InvalidOperationException("Administrator role assignment failed.");
            }
        }
    }
}
