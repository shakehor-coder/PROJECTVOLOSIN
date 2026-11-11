using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class RegistryController(ApplicationRegistry registry) : Controller
{
    public IActionResult Index() => View(registry.Snapshot());
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(string? deliveryNote)
    {
        if (string.IsNullOrWhiteSpace(deliveryNote) || deliveryNote.Trim().Length > 200)
        {
            TempData["RegistryError"] = "Введите примечание длиной от 1 до 200 символов.";
            return RedirectToAction(nameof(Index));
        }
        registry.SetDeliveryNote(deliveryNote);
        return RedirectToAction(nameof(Index));
    }
}
