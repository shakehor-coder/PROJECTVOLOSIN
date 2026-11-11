using Ecommerce.Services;

void Check(bool ok, string message)
{
    if (!ok) throw new Exception("FAIL: " + message);
    Console.WriteLine("PASS: " + message);
}
var analytics = new InventoryAnalytics(new InventoryCalculationGate());
var empty = await analytics.AnalyzeAsync(Array.Empty<InventoryItem>());
Check(empty.Categories.Count == 0 && !empty.UsedParallelCalculation, "empty catalogue");
var small = new[] {new InventoryItem(1, "A", 10.25m, 3), new InventoryItem(1, "A", 20m, 0), new InventoryItem(2, "B", 5m, 4)};
var report = await analytics.AnalyzeAsync(small);
Check(!report.UsedParallelCalculation, "small catalogue uses sequential calculation");
Check(report.Categories[0] == new InventoryCategory(1, "A", 2, 1, 3, 30.75m), "decimal values, empty stock and product count");
Check(report.Categories[1].StockValue == 20m, "separate category aggregation");
var large = Enumerable.Range(0, 10000).Select(i => new InventoryItem(i % 20, "Category " + (i % 20), (i % 100) / 10m, i % 7)).ToArray();
var expected = large.GroupBy(x=>x.CategoryId).OrderBy(g=>g.Key).Select(g => new InventoryCategory(g.Key, g.First().CategoryName, g.Count(), g.Count(x=>x.StockQuantity<=0), g.Sum(x=>(long)x.StockQuantity), g.Sum(x=>x.Price*x.StockQuantity))).ToArray();
var parallel = await analytics.AnalyzeAsync(large);
Check(parallel.UsedParallelCalculation && parallel.Categories.SequenceEqual(expected), "parallel calculation matches independent LINQ reference");
var simultaneous = await Task.WhenAll(Enumerable.Range(0, 8).Select(_=>analytics.AnalyzeAsync(large)));
Check(simultaneous.All(r=>r.Categories.SequenceEqual(expected)), "concurrent requests do not share accumulators");
using var cancelled = new CancellationTokenSource();cancelled.Cancel();
try {await analytics.AnalyzeAsync(large, cancelled.Token);throw new Exception("FAIL: cancellation ignored");}
catch (OperationCanceledException) {Console.WriteLine("PASS: cancelled work is not calculated");}
var sameName = await analytics.AnalyzeAsync(new[] {new InventoryItem(1,"Same",1m,2),new InventoryItem(2,"Same",3m,4)});
Check(sameName.Categories.Count==2, "categories grouped by ID, not display name");
Console.WriteLine("ALL INVENTORY CHECKS PASSED");
