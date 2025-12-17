// SystemInfoService: Lấy thông tin hệ thống (CPU/RAM/Process)
// Sử dụng PerformanceCounter và GC để lấy metrics

using System.Diagnostics;
using RemoteControl.Shared.Models;
// Requires: dotnet add package System.Diagnostics.PerformanceCounter

namespace RemoteControl.Agent.Services;

public class SystemInfoService
{
    private readonly PerformanceCounter? _cpuCounter;
    private readonly PerformanceCounter? _memoryCounter;
    
    // Cache last known values
    private double _lastCpuUsage = 0;
    private double _lastMemoryUsage = 0;
    private DateTime _lastSampleTime = DateTime.MinValue;
    private readonly object _lock = new();

    // ====== Khởi tạo Counter ======
    public SystemInfoService()
    {
        if (OperatingSystem.IsWindows())
        {
            try
            {
                // Processor: % Processor Time: _Total
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                
                // Memory: % Committed Bytes In Use
                _memoryCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

                // Lần gọi đầu tiên để initialize
                _cpuCounter.NextValue();
                _memoryCounter.NextValue();
                
                // Background task để sample liên tục
                Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(1000); // Sample every 1 second
                        UpdateSamples();
                    }
                });
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"[SystemInfoService] Error initializing counters: {ex.Message}");
            }
        }
    }

    private void UpdateSamples()
    {
        lock (_lock)
        {
            try
            {
                if (_cpuCounter != null)
                {
                    var cpuValue = _cpuCounter.NextValue();
                    if (cpuValue > 0) _lastCpuUsage = Math.Round(cpuValue, 2);
                }
                if (_memoryCounter != null)
                {
                    var memValue = _memoryCounter.NextValue();
                    if (memValue > 0) _lastMemoryUsage = Math.Round(memValue, 2);
                }
                _lastSampleTime = DateTime.Now;
            }
            catch { }
        }
    }

    // ====== Lấy tất cả thông tin hệ thống ======
    public SystemInfo GetSystemInfo()
    {
        return new SystemInfo
        {
            CpuUsage = GetCpuUsage(),
            MemoryUsage = GetMemoryUsage(),
            ProcessCount = GetProcessCount(),
            TotalMemoryMB = GetTotalMemoryMB()
        };
    }

    // ====== Lấy CPU Usage (cached) ======
    public double GetCpuUsage()
    {
        lock (_lock)
        {
            return _lastCpuUsage;
        }
    }

    // ====== Lấy RAM Usage (cached) ======
    public double GetMemoryUsage()
    {
        lock (_lock)
        {
            return _lastMemoryUsage;
        }
    }

    // ====== Đếm số Processes ======
    public int GetProcessCount()
    {
        try
        {
            return Process.GetProcesses().Length;
        }
        catch
        {
            return 0;
        }
    }

    // ====== Lấy tổng RAM (MB) ======
    private long GetTotalMemoryMB()
    {
        try
        {
            // Lấy lượng RAM vật lý có sẵn trên máy (gần đúng)
            return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024;
        }
        catch
        {
            return 0;
        }
    }
}
