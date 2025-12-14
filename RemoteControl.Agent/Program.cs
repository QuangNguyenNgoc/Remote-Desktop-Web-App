// RemoteControl.Agent - Entry Point
// Chạy ứng dụng WinForms để xem Webcam

using RemoteControl.Agent.UI;

// ====== Khởi chạy giao diện WinForms ======
Application.SetHighDpiMode(HighDpiMode.SystemAware);
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.Run(new AgentDebugForm());
