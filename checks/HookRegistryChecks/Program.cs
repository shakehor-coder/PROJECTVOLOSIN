global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Ecommerce.Services;
void Check(bool ok, string name) { if (!ok) throw new Exception("FAIL: " + name); Console.WriteLine("PASS: " + name); }
var registry = new ApplicationRegistry();
registry.SetDeliveryNote("Test delivery");
Parallel.For(0,1000,_=>registry.RecordReport(3));
Check(registry.Snapshot().CompletedReports == 1000 && registry.Snapshot().DeliveryNote == "Test delivery", "registry updates are atomic");
try {registry.SetDeliveryNote(new string('x',201));throw new Exception("FAIL: invalid setting accepted");}
catch (ArgumentException) {Console.WriteLine("PASS: setting validation");}
var analytics = new InventoryAnalytics(new InventoryCalculationGate());
var hook = new InventoryReportHook(analytics, registry, NullLogger<InventoryReportHook>.Instance);
await hook.StartAsync(default);
var items = new[] {new InventoryItem(1,"A",10m,2)};
await analytics.AnalyzeAsync(items);
Check(registry.Snapshot().CompletedReports==1001 && registry.Snapshot().LastProductCount==1,"successful calculation triggers hook");
using var cancelled = new CancellationTokenSource();cancelled.Cancel();
try {await analytics.AnalyzeAsync(items,cancelled.Token);}catch(OperationCanceledException){}
try {await analytics.AnalyzeAsync(new[]{new InventoryItem(1,"A",decimal.MaxValue,2)});}catch(OverflowException){}
Check(registry.Snapshot().CompletedReports==1001,"failed and cancelled calculations do not publish success");
await hook.StopAsync(default);
await analytics.AnalyzeAsync(items);
Check(registry.Snapshot().CompletedReports==1001,"shutdown unsubscribes hook");
Check(typeof(InventoryAnalytics).Assembly.GetName().Name=="ShopEase.Analytics","analytics loaded from separate DLL");
Console.WriteLine("ALL HOOK AND REGISTRY CHECKS PASSED");
