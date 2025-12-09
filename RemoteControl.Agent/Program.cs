// RemoteControl.Agent - Entry Point
// Console app kết nối đến Hub và thực thi commands

using RemoteControl.Agent.Services;
using RemoteControl.Shared.Models;

Console.WriteLine("=================================");
Console.WriteLine("  RemoteControl Agent v1.0");
Console.WriteLine("=================================");
Console.WriteLine();


// Test ScreenshotService
Console.WriteLine("[Test] Testing ScreenshotService...");

var screenshotService = new ScreenshotService();
var result = screenshotService.CaptureScreen(quality: 50);

if (!string.IsNullOrEmpty(result.ImageBase64) && result.Format != "error")
{
    Console.WriteLine("[OK] Screenshot captured successfully!");
    Console.WriteLine($"     - Size: {result.Width}x{result.Height}");
    Console.WriteLine($"     - Format: {result.Format}");
    // Console.WriteLine($"     - Base64 length: {result.ImageBase64.Length} characters");
    Console.WriteLine($"     - Captured at: {result.CapturedAt:yyyy-MM-dd HH:mm:ss}");
    
    // Save to file for verification
    var bytes = Convert.FromBase64String(result.ImageBase64);
    var filePath = Path.Combine(Environment.CurrentDirectory, "test_screenshot.jpg");
    File.WriteAllBytes(filePath, bytes);
    Console.WriteLine($"     - Saved to: {filePath}");
}
else
{
    Console.WriteLine("[FAIL] Screenshot capture failed!");
    Console.WriteLine($"     - Format: {result.Format}");
}

Console.WriteLine();

// Test ProcessService
Console.WriteLine("[Test] Testing ProcessService...");
var processService = new ProcessService();

// 1. List Processes
var processResult = processService.ListProcesses();
if (processResult.Success)
{
    Console.WriteLine($"[OK] Process list retrieved! Count: {processResult.Count}");
    Console.WriteLine("     Top 5 processes by name:");
    foreach (var p in processResult.Processes.Take(5))
    {
        Console.WriteLine($"     - [{p.Id}] {p.Name} ({p.MemoryUsageMB:F2} MB)");
    }
}
else
{
    Console.WriteLine($"[FAIL] List processes failed: {processResult.Message}");
}

// 2. Start Process (Notepad)
Console.WriteLine("\n[Test] Starting Notepad...");
bool started = processService.StartProcess("notepad");
if (started)
{
    Console.WriteLine("[OK] Notepad started.");
    
    // Wait for it to start
    Thread.Sleep(2000); 

    // 3. Find and Kill Notepad
    var listAgain = processService.ListProcesses();
    var notepad = listAgain.Processes.FirstOrDefault(p => p.Name.Equals("notepad", StringComparison.OrdinalIgnoreCase));
    
    if (notepad != null)
    {
        Console.WriteLine($"[Found] Notepad is running with ID: {notepad.Id}. Killing it...");
        bool killed = processService.KillProcess(notepad.Id);
        if (killed)
        {
            Console.WriteLine("[OK] Notepad killed successfully.");
        }
        else
        {
            Console.WriteLine("[FAIL] Failed to kill Notepad.");
        }
    }
    else
    {
        Console.WriteLine("[FAIL] Notepad process not found after starting.");
    }
}
else
{
    Console.WriteLine("[FAIL] Failed to start Notepad.");
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
