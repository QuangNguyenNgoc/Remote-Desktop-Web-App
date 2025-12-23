// RemoteControl.Agent - Entry Point (Dual-Mode Support)
// Hỗ trợ 3 mode chạy:
//   --mode=debug   : Debug UI với WinForms (mặc định)
//   --mode=service : Chạy như Windows Service
//   --mode=hidden  : Chạy ẩn không có UI
//
// Argument: --server=IP:PORT để chỉ định server cố định
//   Ví dụ: Agent.exe --mode=hidden --server=192.168.1.100:5048

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteControl.Agent.Helpers;
using RemoteControl.Agent.UI;
using RemoteControl.Agent.Workers;

// ====== Build Configuration First ======
var serverArg = GetServerArg(args);

var configBuilder = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory) // Use exe directory, not current directory
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddCommandLine(args);

// Override SignalR:HubUrl if --server is specified
if (!string.IsNullOrEmpty(serverArg))
{
    var hubUrl = serverArg.StartsWith("http") 
        ? serverArg 
        : $"http://{serverArg}/remotehub";
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["SignalR:HubUrl"] = hubUrl
    });
}

IConfiguration configuration = configBuilder.Build();

// ====== Parse Run Mode (args first, then config) ======
var mode = GetRunMode(args, configuration);

Console.WriteLine($"[Agent] Starting in {mode} mode...");
if (!string.IsNullOrEmpty(serverArg))
{
    Console.WriteLine($"[Agent] Server: {serverArg}");
}

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

// ====== Helper: Parse run mode từ args hoặc config ======
static RunMode GetRunMode(string[] args, IConfiguration? configuration = null)
{
    // Priority 1: Command line args
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
    
    // Priority 2: appsettings.json -> Agent:DefaultMode
    if (configuration != null)
    {
        var defaultMode = configuration["Agent:DefaultMode"];
        if (!string.IsNullOrEmpty(defaultMode))
        {
            if (Enum.TryParse<RunMode>(defaultMode, ignoreCase: true, out var configMode))
            {
                Console.WriteLine($"[Agent] Using DefaultMode from config: {configMode}");
                return configMode;
            }
        }
    }
    
    return RunMode.Debug;
}

// ====== Helper: Parse --server=IP:PORT ======
static string? GetServerArg(string[] args)
{
    foreach (var arg in args)
    {
        if (arg.StartsWith("--server=", StringComparison.OrdinalIgnoreCase))
        {
            return arg.Substring("--server=".Length);
        }
    }
    return null;
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
