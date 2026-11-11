using Ecommerce.Data;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class InventoryController(ApplicationDbContext context, InventoryAnalytics analytics) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // Async I/O first. Do not share the scoped DbContext with Parallel.For workers.
        var items = await context.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new InventoryItem(p.CategoryId, p.Category.Name, p.Price, p.StockQuantity))
            .ToListAsync(cancellationToken);
        var report = await analytics.AnalyzeAsync(items, cancellationToken);
        return View(report);
    }
}
