using System.Diagnostics;

namespace RemoteControl.Agent.Services;

public class ProcessService
{
    // ====== Lấy danh sách process ======
    public ProcessListResult ListProcesses()
    {
        try
        {
            var processes = Process.GetProcesses();
            var processModels = new List<ProcessModel>();

            foreach (var p in processes)
            {
                try
                {
                    processModels.Add(new ProcessModel
                    {
                        Id = p.Id,
                        Name = p.ProcessName,
                        WindowTitle = p.MainWindowTitle,
                        // Memory usage in MB
                        MemoryUsageMB = p.WorkingSet64 / 1024.0 / 1024.0
                    });
                }
                catch
                {
                    // Ignore processes we can't access
                }
            }

            return new ProcessListResult
            {
                Processes = processModels.OrderBy(p => p.Name).ToList(),
                Count = processModels.Count,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new ProcessListResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    // ====== Kill process ======
    public bool KillProcess(int pid)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            process.Kill();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProcessService] Kill failed: {ex.Message}");
            return false;
        }
    }

    // ====== Start process ======
    public bool StartProcess(string name)
    {
        try
        {
            Process.Start(name);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProcessService] Start failed: {ex.Message}");
            return false;
        }
    }
}

public class ProcessListResult
{
    public List<ProcessModel> Processes { get; set; } = new();
    public int Count { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class ProcessModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string WindowTitle { get; set; } = string.Empty;
    public double MemoryUsageMB { get; set; }
}
