using System.Diagnostics;
using System.Text;

namespace Ecommerce.Services;

public record ProcessSnapshot(string State, int? Pid, DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt, int? ExitCode, string Output);

/// <summary>Tracks one child process at a time. All mutable state is protected by a lock.</summary>
public sealed class ProcessMonitor : IDisposable
{
    private readonly object _gate = new();
    private readonly StringBuilder _output = new();
    private Process? _process;
    private string _state = "Idle";
    private int? _pid, _exitCode;
    private DateTimeOffset? _startedAt, _finishedAt;
    private bool _stopRequested, _disposed;
    private const int OutputLimit = 16000;

    public ProcessSnapshot Snapshot()
    {
        lock (_gate)
            return new(_state, _pid, _startedAt, _finishedAt, _exitCode, _output.ToString());
    }

    public bool TryStart(ProcessStartInfo info)
    {
        lock (_gate)
        {
            if (_disposed || _process != null) return false;
            _output.Clear();
            _pid = _exitCode = null;
            _startedAt = DateTimeOffset.UtcNow;
            _finishedAt = null;
            _stopRequested = false;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;
            info.StandardOutputEncoding = Encoding.UTF8;
            info.StandardErrorEncoding = Encoding.UTF8;
            var process = new Process { StartInfo = info };
            try
            {
                if (!process.Start()) throw new InvalidOperationException("Process did not start.");
                _process = process;
                _pid = process.Id;
                _state = "Running";
                _ = ObserveAsync(process);
                return true;
            }
            catch (Exception ex)
            {
                process.Dispose();
                _state = "Failed";
                _finishedAt = DateTimeOffset.UtcNow;
                Append("Start error: " + ex.Message);
                return false;
            }
        }
    }

    private async Task DrainAsync(StreamReader reader, string prefix)
    {
        // Fixed-size reads also bound memory when the child emits a very long line.
        var buffer = new char[1024];
        int count;
        while ((count = await reader.ReadAsync(buffer.AsMemory())) > 0)
            Append(prefix + new string(buffer, 0, count));
    }

    private void Append(string text)
    {
        lock (_gate)
        {
            _output.Append(text);
            if (_output.Length > OutputLimit) _output.Remove(0, _output.Length - OutputLimit);
        }
    }

    private async Task ObserveAsync(Process process)
    {
        try
        {
            // Drain both pipes concurrently so a full stderr pipe cannot block the child.
            await Task.WhenAll(DrainAsync(process.StandardOutput, ""),
                DrainAsync(process.StandardError, "[stderr] "), process.WaitForExitAsync());
            lock (_gate)
            {
                _exitCode = process.ExitCode;
                _state = _stopRequested ? "Stopped" : (_exitCode == 0 ? "Completed" : "Failed");
            }
        }
        catch (Exception ex)
        {
            lock (_gate) _state = "Failed";
            Append("Monitor error: " + ex.Message);
        }
        finally
        {
            lock (_gate)
            {
                _finishedAt = DateTimeOffset.UtcNow;
                _process = null;
                process.Dispose();
            }
        }
    }

    public bool Stop()
    {
        lock (_gate)
        {
            if (_process == null) return false;
            try
            {
                if (_process.HasExited) return false;
                _process.Kill(entireProcessTree: true);
                _stopRequested = true;
                _state = "Stopping";
                return true;
            }
            catch (InvalidOperationException) { return false; } // Child exited during the check.
            catch (System.ComponentModel.Win32Exception ex)
            {
                Append("Stop error: " + ex.Message);
                return false;
            }
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _disposed = true;
            Stop();
        }
    }
}
