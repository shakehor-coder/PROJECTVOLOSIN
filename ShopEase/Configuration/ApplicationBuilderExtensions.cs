namespace Ecommerce.Configuration;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseShopEase(this WebApplication app)
    {
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
        // Public self-registration is not part of the assignments. Admin login is retained.
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value?.TrimEnd('/');
            if (string.Equals(path, "/Identity/Account/Register", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(path, "/Identity/Account/RegisterConfirmation", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            await next();
        });

        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();
        app.MapGet("/health", () => Results.Ok(new { status = "ready" }));

        return app;
    }
}
