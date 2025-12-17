// ProcessService: Quản lý processes (List/Kill/Start)
// Sử dụng Shared Models để đồng bộ với Web/Hub

using System.Diagnostics;
using RemoteControl.Shared.Models;

namespace RemoteControl.Agent.Services;

public class ProcessService
{
    // Cache for CPU usage per process (calculated via sampling)
    private readonly Dictionary<int, double> _cpuCache = new();
    private readonly Dictionary<int, (TimeSpan CpuTime, DateTime SampleTime)> _lastSample = new();
    private readonly object _lock = new();
    private readonly int _processorCount;

    public ProcessService()
    {
        _processorCount = Environment.ProcessorCount;
        
        // Background task to sample CPU usage
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(2000); // Sample every 2 seconds
                UpdateCpuSamples();
            }
        });
    }

    private void UpdateCpuSamples()
    {
        try
        {
            var processes = Process.GetProcesses();
            var currentTime = DateTime.Now;

            lock (_lock)
            {
                foreach (var p in processes)
                {
                    try
                    {
                        var pid = p.Id;
                        var cpuTime = p.TotalProcessorTime;

                        if (_lastSample.TryGetValue(pid, out var lastData))
                        {
                            var elapsed = (currentTime - lastData.SampleTime).TotalMilliseconds;
                            if (elapsed > 0)
                            {
                                var cpuUsed = (cpuTime - lastData.CpuTime).TotalMilliseconds;
                                // CPU% = (cpuUsed / elapsed) / processorCount * 100
                                var cpuPercent = (cpuUsed / elapsed / _processorCount) * 100;
                                _cpuCache[pid] = Math.Min(100, Math.Max(0, cpuPercent));
                            }
                        }

                        _lastSample[pid] = (cpuTime, currentTime);
                    }
                    catch { /* Skip inaccessible processes */ }
                }

                // Clean up dead processes
                var activeIds = processes.Select(p => p.Id).ToHashSet();
                var deadIds = _cpuCache.Keys.Where(id => !activeIds.Contains(id)).ToList();
                foreach (var id in deadIds)
                {
                    _cpuCache.Remove(id);
                    _lastSample.Remove(id);
                }
            }
        }
        catch { }
    }

    // ====== Lấy danh sách process ======
    public ProcessListResult ListProcesses()
    {
        var result = new ProcessListResult();

        try
        {
            var processes = Process.GetProcesses();

            lock (_lock)
            {
                foreach (var p in processes)
                {
                    try
                    {
                        var cpuUsage = _cpuCache.TryGetValue(p.Id, out var cpu) ? cpu : 0;
                        
                        result.Processes.Add(new ProcessInfo
                        {
                            ProcessId = p.Id,
                            ProcessName = p.ProcessName,
                            WindowTitle = p.MainWindowTitle,
                            MemoryUsageMB = p.WorkingSet64 / 1024 / 1024,
                            ThreadCount = p.Threads.Count,
                            CpuUsage = Math.Round(cpuUsage, 1)
                        });
                    }
                    catch { /* Ignore inaccessible processes */ }
                }
            }

            // Sắp xếp theo tên
            result.Processes = result.Processes.OrderBy(p => p.ProcessName).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProcessService] ListProcesses error: {ex.Message}");
        }

        return result;
    }

    // ====== Kill process theo PID ======
    public (bool success, string message) KillProcess(int pid)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            var name = process.ProcessName;
            process.Kill();
            process.WaitForExit(3000);
            return (true, $"Đã diệt process {name} (PID: {pid})");
        }
        catch (ArgumentException)
        {
            return (false, $"Process với PID {pid} không tồn tại");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi khi diệt process: {ex.Message}");
        }
    }

    // ====== Start process ======
    public (bool success, string message) StartProcess(string name)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = name,
                UseShellExecute = true
            });

            if (process != null)
            {
                return (true, $"Đã khởi động {name} (PID: {process.Id})");
            }
            return (true, $"Đã khởi động {name}");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi khi khởi động {name}: {ex.Message}");
        }
    }
}
