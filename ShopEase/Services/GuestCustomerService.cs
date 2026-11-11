using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Services;

/// <summary>A guest customer record has no password, login cookie or registration form.</summary>
public sealed class GuestCustomerService(UserManager<AppUser> users)
{
    private const string SessionKey = "GuestCustomerId";
    public string? CurrentId(HttpContext context) => context.User.Identity?.IsAuthenticated == true
        ? users.GetUserId(context.User) : context.Session.GetString(SessionKey);

    public async Task<string> GetOrCreateIdAsync(HttpContext context)
    {
        var current = CurrentId(context);
        if (current != null && await users.FindByIdAsync(current) != null) return current;
        var id = Guid.NewGuid().ToString("N");
        var guest = new AppUser { Id = id, UserName = "guest-" + id, FullName = "Гость" };
        var result = await users.CreateAsync(guest); // No password: this record cannot sign in.
        if (!result.Succeeded) throw new InvalidOperationException("Could not start guest checkout.");
        context.Session.SetString(SessionKey, id);
        return id;
    }
}
