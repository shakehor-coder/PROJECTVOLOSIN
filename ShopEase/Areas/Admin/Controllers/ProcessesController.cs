using System.Diagnostics;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ProcessesController(ProcessMonitor monitor) : Controller
{
    public IActionResult Index() => View(monitor.Snapshot());
    [HttpGet]
    public IActionResult Status() => Json(monitor.Snapshot());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Start()
    {
        // The command is fixed on the server; no executable or arguments come from the request.
        var info = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet");
        info.ArgumentList.Add("--info");
        if (!monitor.TryStart(info))
            TempData["ProcessMessage"] = "Запуск не выполнен: проверьте состояние и вывод процесса.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Stop()
    {
        if (!monitor.Stop()) TempData["ProcessMessage"] = "Нет работающего процесса или он уже завершился.";
        return RedirectToAction(nameof(Index));
    }
}
