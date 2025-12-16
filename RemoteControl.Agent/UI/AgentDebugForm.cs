// AgentDebugForm: Giao diện kiểm thử toàn diện cho Agent Services
// Fixed: Layout issues, UI freeze, screenshot resize

using RemoteControl.Agent.Handlers;
using RemoteControl.Agent.Services;
using RemoteControl.Shared.Models;
using System.Diagnostics;

namespace RemoteControl.Agent.UI;

public class AgentDebugForm : Form
{
    private readonly CommandHandler _commandHandler;
    private readonly TabControl _tabControl;
    private readonly Label _lblStatus;

    // Services
    private readonly WebCamService _webCamService;
    private readonly KeyLoggerService _keyLoggerService;
    private readonly SignalRClientService _signalRService;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

    // Dashboard
    private Label _lblCpu = null!, _lblRam = null!, _lblUptime = null!, _lblProcessCount = null!;
    private System.Windows.Forms.Timer _sysInfoTimer = null!;

    // Processes
    private ListView _lvProcesses = null!;
    private TextBox _txtStartProc = null!;

    // Screenshot
    private PictureBox _pbScreenshot = null!;
    private Label _lblScreenshotInfo = null!;

    // Webcam
    private PictureBox _pbWebcam = null!;
    private Button _btnStartCam = null!, _btnStopCam = null!;

    // KeyLogs
    private TextBox _txtKeyLogs = null!;
    private Button _btnStartKeyLog = null!, _btnStopKeyLog = null!;
    private System.Windows.Forms.Timer _logTimer = null!;

    public AgentDebugForm(Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _configuration = configuration;
        _commandHandler = new CommandHandler();
        _webCamService = new WebCamService();
        _keyLoggerService = new KeyLoggerService();
        
        // Init SignalR
        _signalRService = new SignalRClientService(_commandHandler, _configuration);
        _signalRService.OnStatusChanged += (msg) => this.Invoke(() => UpdateStatus(msg));
        _signalRService.OnConnectionStateChanged += (state) => this.Invoke(() => 
        {
            this.Text = $"RemoteControl Agent - Developer Console [{state}]";
        });

        // Form Setup - Fixed size behavior
        this.Text = "RemoteControl Agent - Developer Console";
        this.Size = new Size(1000, 700);
        this.MinimumSize = new Size(1000, 700);
        this.MaximumSize = new Size(1000, 700); // Strict fixed size to prevent overflow
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false; // Disable Maximize
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Segoe UI", 9F);
        this.FormClosing += AgentDebugForm_FormClosing;
        this.Load += AgentDebugForm_Load;
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.DoubleBuffered = true;

        // Main Container Panel
        var mainPanel = new Panel { Dock = DockStyle.Fill };
        
        // Status Bar at BOTTOM
        _lblStatus = new Label
        {
            Text = "Ready",
            Dock = DockStyle.Bottom,
            Height = 25,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(5, 0, 0, 0)
        };

        // Tab Control fills remaining space
        _tabControl = new TabControl 
        { 
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            Padding = new Point(8, 4)
        };

        // WinForms LIFO Docking: Add Status LAST so it docks to Bottom FIRST
        mainPanel.Controls.Add(_lblStatus);
        mainPanel.Controls.Add(_tabControl);
        
        this.Controls.Add(mainPanel);

        InitDashboardTab();
        InitProcessTab();
        InitScreenshotTab();
        InitWebcamTab();
        InitKeyLoggerTab();
        InitPowerTab(); // Add Power Tab

        // Disable TabStop for all controls to allow KeyLogger testing
        DisableTabStop(this);
    }
    
    // =================================================================================
    // TAB 6: POWER CONTROL
    // =================================================================================
    private void InitPowerTab()
    {
        var tab = new TabPage("Power") { Padding = new Padding(20) };
        var layout = new FlowLayoutPanel 
        { 
            Dock = DockStyle.Fill, 
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };

        var lblWarning = new Label 
        { 
            Text = "⚠️ WARNING: These actions will affect the host machine immediately!",
            ForeColor = Color.Red,
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 20)
        };

        layout.Controls.Add(lblWarning);
        layout.Controls.Add(CreatePowerBtn("🔒 Lock Workstation", Color.Orange, CommandType.Lock));
        layout.Controls.Add(CreatePowerBtn("💤 Sleep / Suspend", Color.RoyalBlue, CommandType.Sleep));
        layout.Controls.Add(CreatePowerBtn("🔁 Restart Computer", Color.DarkOrchid, CommandType.Restart));
        layout.Controls.Add(CreatePowerBtn("🛑 Shutdown Computer", Color.Crimson, CommandType.Shutdown));

        tab.Controls.Add(layout);
        _tabControl.TabPages.Add(tab);
    }
    
    private Button CreatePowerBtn(string text, Color color, CommandType type)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(250, 45),
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(5),
            Font = new Font("Segoe UI", 11F)
        };
        btn.Click += (s, e) => 
        {
            if (MessageBox.Show($"Are you sure you want to {text}?", "Confirm Power Action", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var result = _commandHandler.HandleCommand(new CommandRequest { Type = type });
                UpdateStatus(result.Message);
            }
        };
        return btn;
    }

    private void DisableTabStop(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            c.TabStop = false;
            if (c.HasChildren) DisableTabStop(c);
        }
    }

    // =================================================================================
    // TAB 1: DASHBOARD
    // =================================================================================
    private void InitDashboardTab()
    {
        var tab = new TabPage("Dashboard") { Padding = new Padding(10) };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        _lblCpu = CreateCard("CPU", "0%", Color.Tomato);
        _lblRam = CreateCard("RAM", "0%", Color.MediumSeaGreen);
        _lblUptime = CreateCard("Uptime", "00:00:00", Color.DodgerBlue);
        _lblProcessCount = CreateCard("Processes", "0", Color.MediumPurple);

        layout.Controls.Add(_lblCpu, 0, 0);
        layout.Controls.Add(_lblRam, 1, 0);
        layout.Controls.Add(_lblUptime, 0, 1);
        layout.Controls.Add(_lblProcessCount, 1, 1);

        tab.Controls.Add(layout);
        _tabControl.TabPages.Add(tab);

        _sysInfoTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _sysInfoTimer.Tick += (s, e) => UpdateSystemInfo();
        _sysInfoTimer.Start();
    }

    private Label CreateCard(string title, string value, Color color)
    {
        return new Label
        {
            Text = $"{title}\n{value}",
            Dock = DockStyle.Fill,
            Margin = new Padding(5),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            BackColor = color,
            ForeColor = Color.White
        };
    }

    private void UpdateSystemInfo()
    {
        try
        {
            var result = _commandHandler.HandleCommand(new CommandRequest { Type = CommandType.GetSystemInfo });
            if (result.Success && result.Data is SystemInfo info)
            {
                _lblCpu.Text = $"CPU\n{info.CpuUsage}%";
                _lblRam.Text = $"RAM\n{info.MemoryUsage}%";
                _lblProcessCount.Text = $"Processes\n{info.ProcessCount}";
                _lblUptime.Text = $"Uptime\n{(DateTime.Now - Process.GetCurrentProcess().StartTime):hh\\:mm\\:ss}";
            }
        }
        catch { }
    }

    // =================================================================================
    // TAB 2: PROCESSES
    // =================================================================================
    private void InitProcessTab()
    {
        var tab = new TabPage("Processes") { Padding = new Padding(5) };
        
        // Toolbar at TOP with fixed height
        var toolbar = new Panel { Dock = DockStyle.Top, Height = 40 };
        var btnRefresh = Btn("Refresh", Color.Teal, 5, 5);
        btnRefresh.Click += (s, e) => RefreshProcessList();
        var btnKill = Btn("End Task", Color.Firebrick, 100, 5);
        btnKill.Click += (s, e) => KillSelectedProcess();
        _txtStartProc = new TextBox { Location = new Point(200, 8), Width = 120, PlaceholderText = "notepad" };
        var btnStart = Btn("Run", Color.Green, 325, 5);
        btnStart.Click += (s, e) => StartProcess(_txtStartProc.Text);
        toolbar.Controls.AddRange(new Control[] { btnRefresh, btnKill, _txtStartProc, btnStart });

        // ListView fills remaining space
        _lvProcesses = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };
        _lvProcesses.Columns.Add("PID", 70);
        _lvProcesses.Columns.Add("Name", 180);
        _lvProcesses.Columns.Add("Memory", 80);
        _lvProcesses.Columns.Add("Title", 350);

        tab.Controls.Add(_lvProcesses);
        tab.Controls.Add(toolbar);
        _tabControl.TabPages.Add(tab);
    }

    // =================================================================================
    // TAB 3: SCREENSHOT
    // =================================================================================
    private void InitScreenshotTab()
    {
        var tab = new TabPage("Screenshot") { Padding = new Padding(5) };
        
        // Bottom bar with fixed height
        var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 40 };
        var btnCapture = Btn("📷 Capture", Color.RoyalBlue, 5, 5);
        btnCapture.Click += (s, e) => CaptureScreenAsync();
        _lblScreenshotInfo = new Label { Location = new Point(110, 10), AutoSize = true };
        bottomBar.Controls.AddRange(new Control[] { btnCapture, _lblScreenshotInfo });

        // PictureBox fills remaining
        _pbScreenshot = new PictureBox 
        { 
            Dock = DockStyle.Fill, 
            SizeMode = PictureBoxSizeMode.Zoom, 
            BackColor = Color.Black
        };

        tab.Controls.Add(_pbScreenshot);
        tab.Controls.Add(bottomBar);
        _tabControl.TabPages.Add(tab);
    }

    // Async capture to prevent UI freeze
    private async void CaptureScreenAsync()
    {
        _lblScreenshotInfo.Text = "Capturing...";
        await Task.Run(() =>
        {
            var result = _commandHandler.HandleCommand(new CommandRequest { Type = CommandType.CaptureScreen });
            if (result.Success && result.Data is ScreenshotResult screen)
            {
                try
                {
                    byte[] bytes = Convert.FromBase64String(screen.ImageBase64);
                    using var ms = new MemoryStream(bytes);
                    var img = Image.FromStream(ms);
                    
                    this.Invoke(() =>
                    {
                        var old = _pbScreenshot.Image;
                        _pbScreenshot.Image = (Image)img.Clone();
                        old?.Dispose();
                        _lblScreenshotInfo.Text = $"{screen.Width}x{screen.Height} | {bytes.Length / 1024}KB";
                    });
                }
                catch { }
            }
        });
    }

    // =================================================================================
    // TAB 4: WEBCAM
    // =================================================================================
    private void InitWebcamTab()
    {
        var tab = new TabPage("Webcam") { Padding = new Padding(5) };

        // Bottom bar with fixed height
        var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 40 };
        _btnStartCam = Btn("▶ Start", Color.SeaGreen, 5, 5);
        _btnStopCam = Btn("⏹ Stop", Color.Gray, 100, 5);
        _btnStopCam.Enabled = false;
        _btnStartCam.Click += (s, e) => StartWebcam();
        _btnStopCam.Click += async (s, e) => await StopWebcamAsync();
        bottomBar.Controls.AddRange(new Control[] { _btnStartCam, _btnStopCam });

        // PictureBox fills remaining
        _pbWebcam = new PictureBox 
        { 
            Dock = DockStyle.Fill, 
            SizeMode = PictureBoxSizeMode.Zoom, 
            BackColor = Color.Black
        };

        tab.Controls.Add(_pbWebcam);
        tab.Controls.Add(bottomBar);
        _tabControl.TabPages.Add(tab);
    }

    private void StartWebcam()
    {
        _btnStartCam.Enabled = false;
        _btnStopCam.Enabled = true;
        _btnStopCam.BackColor = Color.IndianRed;
        
        _webCamService.Start(frame =>
        {
            try
            {
                using var ms = new MemoryStream(frame);
                var img = Image.FromStream(ms);
                _pbWebcam.BeginInvoke(() =>
                {
                    var old = _pbWebcam.Image;
                    _pbWebcam.Image = (Image)img.Clone();
                    old?.Dispose();
                });
            }
            catch { }
        });
        UpdateStatus("Webcam started");
    }

    // Async stop to prevent UI freeze
    private async Task StopWebcamAsync()
    {
        _btnStopCam.Enabled = false;
        UpdateStatus("Stopping webcam...");
        
        await Task.Run(() => _webCamService.Stop());
        
        _btnStartCam.Enabled = true;
        _btnStopCam.BackColor = Color.Gray;
        _pbWebcam.Image = null;
        UpdateStatus("Webcam stopped");
    }

    // =================================================================================
    // TAB 5: KEYLOGGER
    // =================================================================================
    private void InitKeyLoggerTab()
    {
        var tab = new TabPage("KeyLogger") { Padding = new Padding(5) };

        // Top bar with fixed height
        var topBar = new Panel { Dock = DockStyle.Top, Height = 40 };
        _btnStartKeyLog = Btn("▶ Start", Color.DodgerBlue, 5, 5);
        _btnStopKeyLog = Btn("⏹ Stop", Color.Gray, 100, 5);
        _btnStopKeyLog.Enabled = false;
        _btnStartKeyLog.Click += (s, e) => StartKeyLogger();
        _btnStopKeyLog.Click += async (s, e) => await StopKeyLoggerAsync();
        topBar.Controls.AddRange(new Control[] { _btnStartKeyLog, _btnStopKeyLog });

        // TextBox fills remaining
        _txtKeyLogs = new TextBox 
        { 
            Dock = DockStyle.Fill, 
            Multiline = true, 
            ReadOnly = true, 
            BackColor = Color.FromArgb(30, 30, 30), 
            ForeColor = Color.LawnGreen,
            Font = new Font("Consolas", 11),
            ScrollBars = ScrollBars.Both
        };

        _logTimer = new System.Windows.Forms.Timer { Interval = 300 };
        _logTimer.Tick += (s, e) =>
        {
            try
            {
                var logs = _keyLoggerService.GetLogs();
                if (!string.IsNullOrEmpty(logs)) _txtKeyLogs.AppendText(logs);
            }
            catch { }
        };

        tab.Controls.Add(_txtKeyLogs);
        tab.Controls.Add(topBar);
        _tabControl.TabPages.Add(tab);
    }

    private void StartKeyLogger()
    {
        _keyLoggerService.StartLogging();
        _logTimer.Start();
        _btnStartKeyLog.Enabled = false;
        _btnStopKeyLog.Enabled = true;
        _btnStopKeyLog.BackColor = Color.IndianRed;
        UpdateStatus("KeyLogger started");
    }

    // Async stop to prevent UI freeze
    private async Task StopKeyLoggerAsync()
    {
        _btnStopKeyLog.Enabled = false;
        UpdateStatus("Stopping keylogger...");
        _logTimer.Stop();
        
        await Task.Run(() => _keyLoggerService.StopLogging());
        
        _btnStartKeyLog.Enabled = true;
        _btnStopKeyLog.BackColor = Color.Gray;
        UpdateStatus("KeyLogger stopped");
    }

    // =================================================================================
    // HELPERS
    // =================================================================================
    private Button Btn(string text, Color color, int x, int y)
    {
        return new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(90, 28),
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
    }

    private void RefreshProcessList()
    {
        var result = _commandHandler.HandleCommand(new CommandRequest { Type = CommandType.ListProcesses });
        if (result.Success && result.Data is ProcessListResult list)
        {
            _lvProcesses.BeginUpdate();
            _lvProcesses.Items.Clear();
            foreach (var p in list.Processes)
            {
                var item = new ListViewItem(p.ProcessId.ToString());
                item.SubItems.Add(p.ProcessName);
                item.SubItems.Add(p.MemoryUsageMB.ToString());
                item.SubItems.Add(p.WindowTitle);
                _lvProcesses.Items.Add(item);
            }
            _lvProcesses.EndUpdate();
            UpdateStatus($"{list.Processes.Count} processes");
        }
    }

    private void KillSelectedProcess()
    {
        if (_lvProcesses.SelectedItems.Count == 0) return;
        var pid = _lvProcesses.SelectedItems[0].Text;
        var req = new CommandRequest { Type = CommandType.KillProcess };
        req.Parameters["ProcessId"] = pid;
        _commandHandler.HandleCommand(req);
        RefreshProcessList();
    }

    private void StartProcess(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        var req = new CommandRequest { Type = CommandType.StartProcess };
        req.Parameters["ProcessName"] = name;
        _commandHandler.HandleCommand(req);
        _txtStartProc.Clear();
    }

    private void UpdateStatus(string msg) => _lblStatus.Text = $" {msg} [{DateTime.Now:HH:mm:ss}]";

    private async void AgentDebugForm_Load(object? sender, EventArgs e)
    {
        // Get last used server from config
        var lastServerIp = _configuration["SignalR:LastServerIp"];
        var lastPort = 5048;
        int.TryParse(_configuration["SignalR:LastServerPort"], out lastPort);
        if (lastPort == 0) lastPort = 5048;

        // Show connection dialog
        using var dialog = new ServerConnectionDialog(lastServerIp, lastPort);
        var result = dialog.ShowDialog(this);

        if (result == DialogResult.OK)
        {
            // Set the hub URL from dialog
            _signalRService.SetHubUrl(dialog.HubUrl);
            UpdateStatus($"Connecting to {dialog.HubUrl}...");
            
            // Connect
            await _signalRService.ConnectAsync();
        }
        else
        {
            // User cancelled - close app
            UpdateStatus("Connection cancelled by user");
            this.Close();
        }
    }

    private void AgentDebugForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _sysInfoTimer?.Stop();
        _logTimer?.Stop();
        
        // Stop services on background to prevent freeze
        Task.Run(() =>
        {
            try { _webCamService.Stop(); } catch { }
            try { _keyLoggerService.StopLogging(); } catch { }
        });
    }
}
