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

                // Lần gọi đầu tiên thường trả về 0 nên gọi trước 1 lần
                _cpuCounter.NextValue();
                _memoryCounter.NextValue();
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"[SystemInfoService] Error initializing counters: {ex.Message}");
            }
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

    // ====== Lấy CPU Usage ======
    public double GetCpuUsage()
    {
        if (_cpuCounter == null) return 0;
        try
        {
            return Math.Round((double)_cpuCounter.NextValue(), 2);
        }
        catch
        {
            return 0;
        }
    }

    // ====== Lấy RAM Usage (%) ======
    public double GetMemoryUsage()
    {
        // Trả về % Committed Bytes In Use
        if (_memoryCounter == null) return 0;
        try
        {
            return Math.Round((double)_memoryCounter.NextValue(), 2);
        }
        catch
        {
            return 0;
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
