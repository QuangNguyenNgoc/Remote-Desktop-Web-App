// ProcessService: Quản lý processes (List/Kill/Start)
// Sử dụng Shared Models để đồng bộ với Web/Hub

using System.Diagnostics;
using RemoteControl.Shared.Models;

namespace RemoteControl.Agent.Services;

public class ProcessService
{
    // ====== Lấy danh sách process ======
    // Trả về ProcessListResult từ Shared để Web hiểu được
    public ProcessListResult ListProcesses()
    {
        var result = new ProcessListResult();

        try
        {
            var processes = Process.GetProcesses();

            foreach (var p in processes)
            {
                try
                {
                    result.Processes.Add(new ProcessInfo
                    {
                        ProcessId = p.Id,
                        ProcessName = p.ProcessName,
                        WindowTitle = p.MainWindowTitle,
                        MemoryUsageMB = p.WorkingSet64 / 1024 / 1024,  // Convert to MB (long)
                        ThreadCount = p.Threads.Count
                        // CpuUsage cần PerformanceCounter, tạm để 0
                    });
                }
                catch
                {
                    // Ignore processes we can't access (permission denied)
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
            process.WaitForExit(3000); // Chờ tối đa 3s
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
                UseShellExecute = true  // Cho phép mở bằng shell (VD: notepad, chrome)
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
