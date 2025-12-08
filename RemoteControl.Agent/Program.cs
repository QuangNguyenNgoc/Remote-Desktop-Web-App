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
    Console.WriteLine($"     - Base64 length: {result.ImageBase64.Length} characters");
    Console.WriteLine($"     - Captured at: {result.CapturedAt:yyyy-MM-dd HH:mm:ss}");
    
    // Optional: Save to file for verification
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
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
