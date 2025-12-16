namespace RemoteControl.Agent.UI;

/// <summary>
/// Simple dialog for user to input Server IP address
/// </summary>
public class ServerConnectionDialog : Form
{
    private TextBox _txtServerIp = null!;
    private TextBox _txtPort = null!;
    private Button _btnConnect = null!;
    private Button _btnCancel = null!;
    private Label _lblStatus = null!;

    public string ServerIp { get; private set; } = string.Empty;
    public int Port { get; private set; } = 5048;
    public string HubUrl => $"http://{ServerIp}:{Port}/remotehub";

    public ServerConnectionDialog(string? lastServerIp = null, int lastPort = 5048)
    {
        InitializeComponents();
        
        // Pre-fill with last used values
        if (!string.IsNullOrEmpty(lastServerIp))
        {
            _txtServerIp.Text = lastServerIp;
        }
        _txtPort.Text = lastPort.ToString();
    }

    private void InitializeComponents()
    {
        this.Text = "Connect to Server";
        this.Size = new Size(400, 220);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Font = new Font("Segoe UI", 10F);
        this.AcceptButton = _btnConnect;

        var lblTitle = new Label
        {
            Text = "🖥️ Remote Control Agent",
            Location = new Point(20, 15),
            Size = new Size(360, 25),
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 50)
        };

        var lblIp = new Label
        {
            Text = "Server IP:",
            Location = new Point(20, 55),
            Size = new Size(80, 25)
        };

        _txtServerIp = new TextBox
        {
            Location = new Point(110, 52),
            Size = new Size(160, 25),
            Text = "localhost",
            PlaceholderText = "e.g. 192.168.1.100"
        };

        var lblPort = new Label
        {
            Text = "Port:",
            Location = new Point(280, 55),
            Size = new Size(40, 25)
        };

        _txtPort = new TextBox
        {
            Location = new Point(320, 52),
            Size = new Size(55, 25),
            Text = "5048"
        };

        _lblStatus = new Label
        {
            Text = "Enter server IP and click Connect",
            Location = new Point(20, 90),
            Size = new Size(360, 25),
            ForeColor = Color.Gray
        };

        _btnConnect = new Button
        {
            Text = "🔗 Connect",
            Location = new Point(110, 130),
            Size = new Size(110, 35),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnConnect.FlatAppearance.BorderSize = 0;
        _btnConnect.Click += BtnConnect_Click;

        _btnCancel = new Button
        {
            Text = "Cancel",
            Location = new Point(230, 130),
            Size = new Size(90, 35),
            FlatStyle = FlatStyle.Flat
        };
        _btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        this.Controls.AddRange(new Control[] 
        { 
            lblTitle, lblIp, _txtServerIp, lblPort, _txtPort, 
            _lblStatus, _btnConnect, _btnCancel 
        });

        // Enter key submits
        _txtServerIp.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnConnect_Click(s, e); };
        _txtPort.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnConnect_Click(s, e); };
    }

    private void BtnConnect_Click(object? sender, EventArgs e)
    {
        var ip = _txtServerIp.Text.Trim();
        
        if (string.IsNullOrEmpty(ip))
        {
            _lblStatus.Text = "❌ Please enter Server IP";
            _lblStatus.ForeColor = Color.Red;
            return;
        }

        if (!int.TryParse(_txtPort.Text.Trim(), out int port) || port < 1 || port > 65535)
        {
            _lblStatus.Text = "❌ Invalid port number";
            _lblStatus.ForeColor = Color.Red;
            return;
        }

        ServerIp = ip;
        Port = port;

        _lblStatus.Text = $"✅ Connecting to {HubUrl}...";
        _lblStatus.ForeColor = Color.Green;

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
