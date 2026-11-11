using Ecommerce.Services;

void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name);
}
TaskCompletionSource Signal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
var gate = new InventoryCalculationGate();
int active = 0, maxActive = 0;
using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(20));
var token = deadline.Token;

// Hold the first calculation so every other request is guaranteed to encounter an occupied gate.
var entered = Signal();
var release = Signal();
var first = gate.ExecuteAsync(async ct =>
{
    Interlocked.Increment(ref active);
    entered.SetResult();
    try { await release.Task.WaitAsync(ct); return -1; }
    finally { Interlocked.Decrement(ref active); }
}, token);
await entered.Task.WaitAsync(token);
var queued = Enumerable.Range(0, 30).Select(i => gate.ExecuteAsync(async ct =>
{
    int count = Interlocked.Increment(ref active);
    int old;
    do { old = Volatile.Read(ref maxActive); }
    while (count > old && Interlocked.CompareExchange(ref maxActive, count, old) != old);
    try { await Task.Delay(5, ct); return i; }
    finally { Interlocked.Decrement(ref active); }
}, token)).ToArray();
Check(queued.All(t => !t.IsCompleted) && active == 1, "occupied gate queues callers without entering calculation");

using var cancelled = new CancellationTokenSource();
bool cancelledDelegateRan = false;
var waiter = gate.ExecuteAsync(ct => { cancelledDelegateRan = true; return Task.FromResult(0); }, cancelled.Token);
cancelled.Cancel();
try { await waiter; throw new Exception("FAIL: waiter cancellation ignored"); }
catch (OperationCanceledException) { Check(!cancelledDelegateRan, "cancelled waiter never enters or releases an unowned permit"); }
release.SetResult();
Check(await first == -1, "first calculation completes");
var results = await Task.WhenAll(queued).WaitAsync(token);
Check(maxActive == 1 && active == 0 && results.SequenceEqual(Enumerable.Range(0, 30)), "30 queued calculations are mutually exclusive and preserve their results");

try
{
    await gate.ExecuteAsync<int>(ct => throw new InvalidOperationException("Expected test exception"), token);
    throw new Exception("FAIL: exception lost");
}
catch (InvalidOperationException) { Console.WriteLine("PASS: calculation exception propagated"); }
Check(await gate.ExecuteAsync(ct => Task.FromResult(42), token) == 42, "exception releases permit for next caller");

using var during = new CancellationTokenSource();
var running = Signal();
var cancelledWork = gate.ExecuteAsync(async ct =>
{
    running.SetResult();
    await Task.Delay(Timeout.Infinite, ct);
    return 0;
}, during.Token);
await running.Task.WaitAsync(token);
during.Cancel();
try { await cancelledWork; throw new Exception("FAIL: running cancellation ignored"); }
catch (OperationCanceledException) { Console.WriteLine("PASS: cancellation during calculation propagated"); }
Check(await gate.ExecuteAsync(ct => Task.FromResult(99), token) == 99, "cancellation inside calculation releases permit");
Console.WriteLine("ALL SYNCHRONIZATION CHECKS PASSED");
