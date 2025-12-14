// WebCamForm: Giao diện xem Webcam và KeyLogger
// Form WinForms hiển thị stream từ WebCamService và log phím từ KeyLoggerService

using RemoteControl.Agent.Services;

namespace RemoteControl.Agent.UI;

public class WebCamForm : Form
{
    // ====== WebCam ======
    private readonly WebCamService _webCamService;
    private readonly PictureBox _pictureBox;
    private readonly Button _btnStartCam;
    private readonly Button _btnStopCam;

    // ====== KeyLogger ======
    private readonly KeyLoggerService _keyLoggerService;
    private readonly TextBox _txtKeyLogs;
    private readonly Button _btnStartKeyLog;
    private readonly Button _btnStopKeyLog;
    private readonly System.Windows.Forms.Timer _logTimer;

    private readonly Label _lblStatus;

    public WebCamForm()
    {
        _webCamService = new WebCamService();
        _keyLoggerService = new KeyLoggerService();

        // ====== Thiết lập Form ======
        this.Text = "RemoteControl - Agent Test UI";
        this.Size = new Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormClosing += WebCamForm_FormClosing;

        // ====== PictureBox hiển thị Video ======
        _pictureBox = new PictureBox
        {
            Location = new Point(10, 10),
            Size = new Size(600, 400),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black
        };
        this.Controls.Add(_pictureBox);

        // ====== Nút Start Camera ======
        _btnStartCam = new Button
        {
            Text = "▶ Start Camera",
            Location = new Point(10, 420),
            Size = new Size(140, 35),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _btnStartCam.Click += BtnStartCam_Click;
        this.Controls.Add(_btnStartCam);

        // ====== Nút Stop Camera ======
        _btnStopCam = new Button
        {
            Text = "⏹ Stop Camera",
            Location = new Point(160, 420),
            Size = new Size(140, 35),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        _btnStopCam.Click += BtnStopCam_Click;
        this.Controls.Add(_btnStopCam);

        // ====== KeyLogger Panel ======
        var lblKeyLogger = new Label
        {
            Text = "⌨️ KeyLogger",
            Location = new Point(620, 10),
            Size = new Size(360, 25),
            Font = new Font("Segoe UI", 11, FontStyle.Bold)
        };
        this.Controls.Add(lblKeyLogger);

        _txtKeyLogs = new TextBox
        {
            Location = new Point(620, 40),
            Size = new Size(360, 300),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            Font = new Font("Consolas", 10),
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.Lime
        };
        this.Controls.Add(_txtKeyLogs);

        // ====== Nút Start KeyLogger ======
        _btnStartKeyLog = new Button
        {
            Text = "▶ Start KeyLogger",
            Location = new Point(620, 350),
            Size = new Size(170, 35),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _btnStartKeyLog.Click += BtnStartKeyLog_Click;
        this.Controls.Add(_btnStartKeyLog);

        // ====== Nút Stop KeyLogger ======
        _btnStopKeyLog = new Button
        {
            Text = "⏹ Stop KeyLogger",
            Location = new Point(800, 350),
            Size = new Size(170, 35),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(155, 89, 182),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        _btnStopKeyLog.Click += BtnStopKeyLog_Click;
        this.Controls.Add(_btnStopKeyLog);

        // ====== Timer để cập nhật log ======
        _logTimer = new System.Windows.Forms.Timer { Interval = 500 };
        _logTimer.Tick += LogTimer_Tick;

        // ====== Label trạng thái ======
        _lblStatus = new Label
        {
            Text = "Status: Ready",
            Location = new Point(10, 620),
            Size = new Size(960, 30),
            Font = new Font("Segoe UI", 9)
        };
        this.Controls.Add(_lblStatus);
    }

    // ====== Camera Controls ======
    private void BtnStartCam_Click(object? sender, EventArgs e)
    {
        _btnStartCam.Enabled = false;
        _btnStopCam.Enabled = true;
        _lblStatus.Text = "Status: Camera Running";
        _webCamService.Start(OnFrameReceived);
    }

    private void BtnStopCam_Click(object? sender, EventArgs e)
    {
        _webCamService.Stop();
        _btnStartCam.Enabled = true;
        _btnStopCam.Enabled = false;
        _lblStatus.Text = "Status: Camera Stopped";
    }

    private void OnFrameReceived(byte[] frameBytes)
    {
        try
        {
            using var ms = new MemoryStream(frameBytes);
            var image = Image.FromStream(ms);

            if (_pictureBox.InvokeRequired)
            {
                _pictureBox.Invoke(() =>
                {
                    var oldImage = _pictureBox.Image;
                    _pictureBox.Image = (Image)image.Clone();
                    oldImage?.Dispose();
                });
            }
            else
            {
                var oldImage = _pictureBox.Image;
                _pictureBox.Image = (Image)image.Clone();
                oldImage?.Dispose();
            }
        }
        catch { }
    }

    // ====== KeyLogger Controls ======
    private void BtnStartKeyLog_Click(object? sender, EventArgs e)
    {
        _btnStartKeyLog.Enabled = false;
        _btnStopKeyLog.Enabled = true;
        _txtKeyLogs.Text = "";
        _keyLoggerService.StartLogging();
        _logTimer.Start();
        _lblStatus.Text = "Status: KeyLogger Running - Type anywhere!";
    }

    private void BtnStopKeyLog_Click(object? sender, EventArgs e)
    {
        _logTimer.Stop();
        _keyLoggerService.StopLogging();
        _btnStartKeyLog.Enabled = true;
        _btnStopKeyLog.Enabled = false;
        _lblStatus.Text = "Status: KeyLogger Stopped";
    }

    private void LogTimer_Tick(object? sender, EventArgs e)
    {
        string logs = _keyLoggerService.GetLogs();
        if (!string.IsNullOrEmpty(logs))
        {
            _txtKeyLogs.AppendText(logs);
        }
    }

    // ====== Cleanup ======
    private void WebCamForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _logTimer.Stop();
        _webCamService.Stop();
        _keyLoggerService.StopLogging();
    }
}
