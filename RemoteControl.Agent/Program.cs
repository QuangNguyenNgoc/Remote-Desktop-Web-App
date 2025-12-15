// RemoteControl.Agent - Entry Point
// Chạy ứng dụng WinForms để xem Webcam

using RemoteControl.Agent.UI;
using Microsoft.Extensions.Configuration;

// ====== Khởi chạy giao diện WinForms ======
Application.SetHighDpiMode(HighDpiMode.SystemAware);
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);

// Build Configuration
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddCommandLine(args);

IConfiguration configuration = builder.Build();

Application.Run(new AgentDebugForm(configuration));
