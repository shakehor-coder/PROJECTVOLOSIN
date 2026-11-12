using Ecommerce.Data;
using Ecommerce.Services;
using Ecommerce.Models;
using Ecommerce.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShopEase(this IServiceCollection services,
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Add services to the container.
        var provider = configuration["Database:Provider"] ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
            Directory.CreateDirectory(dataDirectory);
            if (string.IsNullOrWhiteSpace(connectionString))
                connectionString = "Data Source=" + Path.Combine(dataDirectory, "shop.db");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
        }
        else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection or select Database:Provider=Sqlite.");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        }
        else throw new InvalidOperationException("Unsupported Database:Provider.");
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddIdentity<AppUser, IdentityRole>(options =>
        options.SignIn.RequireConfirmedAccount = false)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddDefaultUI();

        services.AddControllersWithViews();
        services.AddSingleton<ProcessMonitor>();
        services.AddSingleton<ApplicationRegistry>();
        services.AddSingleton<ProductImageCatalog>();
        services.AddSingleton<InventoryCalculationGate>();
        services.AddSingleton<InventoryAnalytics>();
        services.AddHostedService<InventoryReportHook>();

        //redirect to login when not authenticated
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Identity/Account/Login";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        });

        services.AddScoped<GuestCustomerService>();
        services.AddScoped<CheckoutService>();

        //register repositories
        services.AddScoped(typeof(IEntityRepository<,>), typeof(EntityRepository<,>));
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();

        var keyDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "keys");
        Directory.CreateDirectory(keyDirectory);
        services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));

        //session for cart
        services.AddDistributedMemoryCache();
        services.AddSession();

        return services;
    }
}
