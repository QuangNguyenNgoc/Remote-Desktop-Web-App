// RemoteControl.Agent - Entry Point
// Console app kết nối đến Hub và thực thi commands

using RemoteControl.Agent.Services;
using RemoteControl.Shared.Models;

Console.WriteLine("=================================");
Console.WriteLine("  RemoteControl Agent v1.0");
Console.WriteLine("=================================");
Console.WriteLine();

var systemInfoService = new SystemInfoService();

var sysInfo = systemInfoService.GetSystemInfo();
Console.WriteLine($"[OK] System Info retrieved:");
Console.WriteLine($"- CPU Usage: {sysInfo.CpuUsage}%");
Console.WriteLine($"- Memory Usage: {sysInfo.MemoryUsage}%");
Console.WriteLine($"- Total Memory: {sysInfo.TotalMemoryMB} MB");
Console.WriteLine($"- Process Count: {sysInfo.ProcessCount}");

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

