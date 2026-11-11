namespace Ecommerce.Services;

/// <summary>Application event hook, attached and detached with the host lifecycle.</summary>
public sealed class InventoryReportHook(InventoryAnalytics analytics, ApplicationRegistry registry,
    ILogger<InventoryReportHook> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        analytics.ReportCompleted += OnReportCompleted;
        return Task.CompletedTask;
    }
    private void OnReportCompleted(object? sender, InventoryReportEventArgs args)
    {
        registry.RecordReport(args.ProductCount);
        logger.LogInformation("Inventory report completed: {ProductCount} products", args.ProductCount);
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        analytics.ReportCompleted -= OnReportCompleted;
        return Task.CompletedTask;
    }
}
