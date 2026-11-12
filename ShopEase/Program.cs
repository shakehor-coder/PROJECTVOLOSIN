using System.Globalization;
using Ecommerce.Configuration;
using Ecommerce.Data;

namespace Ecommerce;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Demo prices are USD on every host.
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddShopEase(builder.Configuration, builder.Environment);

        var app = builder.Build();
        app.UseShopEase();
        await app.InitializeShopEaseAsync();
        await app.RunAsync();
    }
}
