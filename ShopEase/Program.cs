using Ecommerce.Data;
using Ecommerce.Services;
using Ecommerce.Models;
using Ecommerce.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var provider = builder.Configuration["Database:Provider"] ?? "SqlServer";
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
                Directory.CreateDirectory(dataDirectory);
                if (string.IsNullOrWhiteSpace(connectionString))
                    connectionString = "Data Source=" + Path.Combine(dataDirectory, "shop.db");
                builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
            }
            else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection or select Database:Provider=Sqlite.");
                builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            }
            else throw new InvalidOperationException("Unsupported Database:Provider.");
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<ProcessMonitor>();
            builder.Services.AddSingleton<ApplicationRegistry>();
            builder.Services.AddSingleton<InventoryCalculationGate>();
            builder.Services.AddSingleton<InventoryAnalytics>();
            builder.Services.AddHostedService<InventoryReportHook>();

            //redirect to login when not authenticated
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            //register repositories
            builder.Services.AddScoped(typeof(IEntityRepository<,>), typeof(EntityRepository<,>));
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IAddressRepository, AddressRepository>();

            var keyDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys");
            Directory.CreateDirectory(keyDirectory);
            builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));

            //session for cart
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
            app.MapGet("/health", () => Results.Ok(new { status = "ready" }));

            // Seed roles
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (context.Database.IsSqlite()) await context.Database.EnsureCreatedAsync();
                else await context.Database.MigrateAsync();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                string[] roles = { "Admin", "Customer" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                // Seed admin account
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var adminEmail = builder.Configuration["SeedAdmin:Email"];
                var adminPassword = builder.Configuration["SeedAdmin:Password"];
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

                // Seed sample data
                DbInitializer.Seed(context);
            }

            app.Run();
        }
    }
}
