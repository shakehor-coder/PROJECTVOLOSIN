using System.Diagnostics;
using Ecommerce.Services;
using System.Reflection;

if (args.FirstOrDefault() == "--child")
{
    Console.WriteLine("stdout marker");
    Console.Error.WriteLine("stderr marker");
    if (args[1] == "wait") await Task.Delay(15000);
    if (args[1] == "large")
    {
        await Task.WhenAll(Console.Out.WriteAsync(new string('A', 50000)),
            Console.Error.WriteAsync(new string('B', 50000)));
    }
    return args[1] == "fail" ? 7 : 0;
}
var host = Environment.ProcessPath!;
ProcessStartInfo Child(string mode)
{
    var info = new ProcessStartInfo(host);
    if (Path.GetFileNameWithoutExtension(host).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        info.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
    info.ArgumentList.Add("--child"); info.ArgumentList.Add(mode);
    return info;
}
void Check(bool condition, string text)
{
    if (!condition) throw new Exception("FAIL: " + text);
    Console.WriteLine("PASS: " + text);
}
async Task<ProcessSnapshot> Finished(ProcessMonitor monitor)
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(12));
    while (monitor.Snapshot().State is "Running" or "Stopping")
        await Task.Delay(20, timeout.Token);
    return monitor.Snapshot();
}
using var monitor = new ProcessMonitor();
Check(monitor.Snapshot().State == "Idle", "initial idle state");
Check(monitor.TryStart(Child("ok")), "start child");
var s = await Finished(monitor);
Check(s.State == "Completed" && s.ExitCode == 0 && s.Pid > 0 && s.FinishedAt >= s.StartedAt,
    "completion, PID, exit code and timestamps");
Check(s.Output.Contains("stdout marker") && s.Output.Contains("stderr marker"), "both output streams captured");
Check(monitor.TryStart(Child("fail")), "restart after completion");
s = await Finished(monitor);
Check(s.State == "Failed" && s.ExitCode == 7, "nonzero exit reported");
Check(monitor.TryStart(Child("wait")), "start long child");
Check(monitor.Snapshot().State == "Running" && !monitor.TryStart(Child("ok")), "running visible, duplicate start rejected");
Check(monitor.Stop(), "stop request accepted");
s = await Finished(monitor);
Check(s.State == "Stopped" && s.ExitCode.HasValue, "stopped child is reaped");
Check(monitor.TryStart(Child("large")), "start large output child");
s = await Finished(monitor);
Check(s.State == "Completed" && s.Output.Length <= 16000, "concurrent pipes drained and output bounded");
Check(!monitor.TryStart(new ProcessStartInfo("/missing-shopease-test-executable")), "missing executable handled");
Check(monitor.Snapshot().State == "Failed" && monitor.Snapshot().Pid == null, "start failure has no stale PID");
using var shutdown = new ProcessMonitor();
Check(shutdown.TryStart(Child("wait")), "start shutdown child");
shutdown.Dispose();
s = await Finished(shutdown);
Check(s.State == "Stopped" && !shutdown.TryStart(Child("ok")), "dispose stops child and rejects new starts");
Console.WriteLine("ALL CHECKS PASSED");
return 0;
