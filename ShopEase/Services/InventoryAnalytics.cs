namespace Ecommerce.Services;

// Immutable values copied from the database. No EF entities or DbContext cross thread boundaries.
public sealed record InventoryItem(int CategoryId, string CategoryName, decimal Price, int StockQuantity);
public sealed record InventoryCategory(int CategoryId, string CategoryName, int ProductCount,
    int OutOfStockCount, long Units, decimal StockValue);
public sealed record InventoryReport(IReadOnlyList<InventoryCategory> Categories,
    bool UsedParallelCalculation);

public sealed class InventoryAnalytics(InventoryCalculationGate calculationGate)
{
    public const int ParallelThreshold = 1000;
    public const int MaxWorkers = 4;

    public Task<InventoryReport> AnalyzeAsync(IReadOnlyList<InventoryItem> items,
        CancellationToken cancellationToken = default)
    {
        // Copy before scheduling: callers cannot mutate the collection during computation.
        var snapshot = items.ToArray();
        return calculationGate.ExecuteAsync(
            token => Task.Run(() => Calculate(snapshot, token), token), cancellationToken);
    }

    private static InventoryReport Calculate(InventoryItem[] items, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var groups = items.GroupBy(p => p.CategoryId).OrderBy(g => g.Key).ToArray();
        var result = new InventoryCategory[groups.Length];
        var useParallel = items.Length >= ParallelThreshold && groups.Length > 1;
        void CalculateCategory(int index)
        {
            var group = groups[index];
            int count = 0, empty = 0;
            long units = 0;
            decimal value = 0;
            foreach (var product in group)
            {
                token.ThrowIfCancellationRequested();
                count++;
                if (product.StockQuantity <= 0) empty++;
                units = checked(units + product.StockQuantity);
                value += product.Price * product.StockQuantity;
            }
            // Each worker owns one array slot and its own accumulators.
            result[index] = new InventoryCategory(group.Key, group.First().CategoryName,
                count, empty, units, value);
        }
        if (useParallel)
        {
            Parallel.For(0, groups.Length, new ParallelOptions
            {
                CancellationToken = token,
                MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, MaxWorkers)
            }, CalculateCategory);
        }
        else
        {
            for (int i = 0; i < groups.Length; i++) CalculateCategory(i);
        }
        token.ThrowIfCancellationRequested();
        return new InventoryReport(Array.AsReadOnly(result), useParallel);
    }
}
