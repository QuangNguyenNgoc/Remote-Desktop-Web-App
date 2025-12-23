using System.Text.Json;

namespace RemoteControl.Web.Services;

public class TranslationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new();
    private string _currentLanguage = "EN";

    public TranslationService()
    {
        LoadTranslations();
    }

    public string CurrentLanguage => _currentLanguage;

    public event Action? OnLanguageChanged;

    public void SetLanguage(string lang)
    {
        if (_translations.ContainsKey(lang.ToUpper()))
        {
            _currentLanguage = lang.ToUpper();
            OnLanguageChanged?.Invoke();
        }
    }

    public string T(string key)
    {
        if (_translations.TryGetValue(_currentLanguage, out var langDict))
        {
            if (langDict.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        
        // Fallback to English
        if (_translations.TryGetValue("EN", out var enDict))
        {
            if (enDict.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        
        return key; // Return key if not found
    }

    public IEnumerable<string> AvailableLanguages => _translations.Keys;

    private void LoadTranslations()
    {
        // English
        _translations["EN"] = new Dictionary<string, string>
        {
            // Navigation
            { "nav_dashboard", "Dashboard" },
            { "nav_devices", "Device Manager" },
            { "nav_settings", "Settings" },
            
            // Dashboard
            { "dashboard_title", "Dashboard" },
            { "dashboard_total", "Total Devices" },
            { "dashboard_online", "Online" },
            { "dashboard_offline", "Offline" },
            { "dashboard_commands", "Commands Sent" },
            { "dashboard_connection_status", "Connection Status" },
            { "dashboard_connected", "Connected" },
            { "dashboard_disconnected", "Disconnected" },
            { "dashboard_agents", "Active Agents" },
            { "dashboard_no_agents", "No agents connected" },
            
            // Device Manager
            { "devices_title", "Device Manager" },
            { "devices_search", "Search devices..." },
            { "devices_refresh", "Refresh" },
            { "devices_no_devices", "No devices found" },
            { "devices_connect", "Connect" },
            { "devices_disconnect", "Disconnect" },
            { "devices_status", "Status" },
            { "devices_name", "Device Name" },
            { "devices_ip", "IP Address" },
            { "devices_os", "Operating System" },
            { "devices_last_seen", "Last Seen" },
            { "devices_actions", "Actions" },
            
            // Settings
            { "settings_title", "Settings" },
            { "settings_language", "Language" },
            { "settings_language_desc", "Select interface language" },
            { "settings_appearance", "Appearance" },
            { "settings_theme", "Theme" },
            { "settings_dark_mode", "Dark Mode" },
            { "settings_light_mode", "Light Mode" },
            { "settings_wallpaper", "Wallpaper" },
            { "settings_wallpaper_default", "Default" },
            { "settings_wallpaper_sunset", "Sunset" },
            { "settings_wallpaper_ocean", "Ocean" },
            { "settings_wallpaper_forest", "Forest" },
            { "settings_wallpaper_purple", "Purple" },
            { "settings_wallpaper_midnight", "Midnight" },
            { "settings_notifications", "Notifications" },
            { "settings_push_notifications", "Push Notifications" },
            { "settings_sound_alerts", "Sound Alerts" },
            { "settings_connection", "Connection" },
            { "settings_auto_reconnect", "Auto Reconnect" },
            { "settings_low_bandwidth", "Low Bandwidth Mode" },
            { "settings_refresh_interval", "Refresh Interval" },
            { "settings_about", "About" },
            { "settings_version", "Version" },
            { "settings_app_name", "Application" },
            { "settings_framework", "Framework" },
            
            // Login
            { "login_title", "NETSPY" },
            { "login_subtitle", "Admin authentication required" },
            { "login_password", "Password" },
            { "login_placeholder", "Enter admin password" },
            { "login_remember", "Remember me" },
            { "login_remember_hint", "(1 hour)" },
            { "login_button", "Login" },
            { "login_loading", "Logging in..." },
            { "login_error_empty", "Please enter password." },
            { "login_error_wrong", "Wrong password. Please try again." },
            { "login_error_locked", "Too many attempts. Please wait" },
            { "login_server", "Server" },
            
            // Common
            { "common_save", "Save" },
            { "common_cancel", "Cancel" },
            { "common_delete", "Delete" },
            { "common_edit", "Edit" },
            { "common_close", "Close" },
            { "common_loading", "Loading..." },
            { "common_error", "Error" },
            { "common_success", "Success" },
            { "common_warning", "Warning" },
            { "common_info", "Info" },
            { "common_seconds", "seconds" },
            { "common_coming_soon", "Coming Soon" },
            
            // Device Control
            { "control_remote_screen", "Remote Screen" },
            { "control_task_manager", "Task Manager" },
            { "control_system_monitor", "System Monitor" },
            { "control_keylogger", "Keylogger" },
            { "control_webcam", "Webcam" },
            { "control_power", "Power" },
            { "control_registry", "Registry" },
            { "control_start", "Start" },
            { "control_stop", "Stop" },
            { "control_capture", "Capture" },
            { "control_screenshot", "Screenshot" }
        };

        // Vietnamese
        _translations["VN"] = new Dictionary<string, string>
        {
            // Navigation
            { "nav_dashboard", "Bảng điều khiển" },
            { "nav_devices", "Quản lý thiết bị" },
            { "nav_settings", "Cài đặt" },
            
            // Dashboard
            { "dashboard_title", "Bảng điều khiển" },
            { "dashboard_total", "Tổng thiết bị" },
            { "dashboard_online", "Trực tuyến" },
            { "dashboard_offline", "Ngoại tuyến" },
            { "dashboard_commands", "Lệnh đã gửi" },
            { "dashboard_connection_status", "Trạng thái kết nối" },
            { "dashboard_connected", "Đã kết nối" },
            { "dashboard_disconnected", "Ngắt kết nối" },
            { "dashboard_agents", "Agent hoạt động" },
            { "dashboard_no_agents", "Không có agent nào kết nối" },
            
            // Device Manager
            { "devices_title", "Quản lý thiết bị" },
            { "devices_search", "Tìm kiếm thiết bị..." },
            { "devices_refresh", "Làm mới" },
            { "devices_no_devices", "Không tìm thấy thiết bị" },
            { "devices_connect", "Kết nối" },
            { "devices_disconnect", "Ngắt kết nối" },
            { "devices_status", "Trạng thái" },
            { "devices_name", "Tên thiết bị" },
            { "devices_ip", "Địa chỉ IP" },
            { "devices_os", "Hệ điều hành" },
            { "devices_last_seen", "Lần cuối" },
            { "devices_actions", "Thao tác" },
            
            // Settings
            { "settings_title", "Cài đặt" },
            { "settings_language", "Ngôn ngữ" },
            { "settings_language_desc", "Chọn ngôn ngữ giao diện" },
            { "settings_appearance", "Giao diện" },
            { "settings_theme", "Chủ đề" },
            { "settings_dark_mode", "Chế độ tối" },
            { "settings_light_mode", "Chế độ sáng" },
            { "settings_wallpaper", "Hình nền" },
            { "settings_wallpaper_default", "Mặc định" },
            { "settings_wallpaper_sunset", "Hoàng hôn" },
            { "settings_wallpaper_ocean", "Đại dương" },
            { "settings_wallpaper_forest", "Rừng xanh" },
            { "settings_wallpaper_purple", "Tím" },
            { "settings_wallpaper_midnight", "Nửa đêm" },
            { "settings_notifications", "Thông báo" },
            { "settings_push_notifications", "Thông báo đẩy" },
            { "settings_sound_alerts", "Âm thanh cảnh báo" },
            { "settings_connection", "Kết nối" },
            { "settings_auto_reconnect", "Tự động kết nối lại" },
            { "settings_low_bandwidth", "Chế độ băng thông thấp" },
            { "settings_refresh_interval", "Khoảng thời gian làm mới" },
            { "settings_about", "Thông tin" },
            { "settings_version", "Phiên bản" },
            { "settings_app_name", "Ứng dụng" },
            { "settings_framework", "Framework" },
            
            // Login
            { "login_title", "NETSPY" },
            { "login_subtitle", "Yêu cầu xác thực quản trị viên" },
            { "login_password", "Mật khẩu" },
            { "login_placeholder", "Nhập mật khẩu quản trị" },
            { "login_remember", "Ghi nhớ đăng nhập" },
            { "login_remember_hint", "(1 giờ)" },
            { "login_button", "Đăng nhập" },
            { "login_loading", "Đang đăng nhập..." },
            { "login_error_empty", "Vui lòng nhập mật khẩu." },
            { "login_error_wrong", "Sai mật khẩu. Vui lòng thử lại." },
            { "login_error_locked", "Quá nhiều lần thử. Vui lòng đợi" },
            { "login_server", "Máy chủ" },
            
            // Common
            { "common_save", "Lưu" },
            { "common_cancel", "Hủy" },
            { "common_delete", "Xóa" },
            { "common_edit", "Sửa" },
            { "common_close", "Đóng" },
            { "common_loading", "Đang tải..." },
            { "common_error", "Lỗi" },
            { "common_success", "Thành công" },
            { "common_warning", "Cảnh báo" },
            { "common_info", "Thông tin" },
            { "common_seconds", "giây" },
            { "common_coming_soon", "Sắp ra mắt" },
            
            // Device Control
            { "control_remote_screen", "Màn hình từ xa" },
            { "control_task_manager", "Quản lý tác vụ" },
            { "control_system_monitor", "Giám sát hệ thống" },
            { "control_keylogger", "Ghi phím" },
            { "control_webcam", "Webcam" },
            { "control_power", "Nguồn" },
            { "control_registry", "Registry" },
            { "control_start", "Bắt đầu" },
            { "control_stop", "Dừng" },
            { "control_capture", "Chụp" },
            { "control_screenshot", "Ảnh màn hình" }
        };

        // Japanese
        _translations["JP"] = new Dictionary<string, string>
        {
            // Navigation
            { "nav_dashboard", "ダッシュボード" },
            { "nav_devices", "デバイス管理" },
            { "nav_settings", "設定" },
            
            // Dashboard
            { "dashboard_title", "ダッシュボード" },
            { "dashboard_total", "合計デバイス" },
            { "dashboard_online", "オンライン" },
            { "dashboard_offline", "オフライン" },
            { "dashboard_commands", "送信コマンド" },
            { "dashboard_connection_status", "接続状態" },
            { "dashboard_connected", "接続済み" },
            { "dashboard_disconnected", "切断" },
            { "dashboard_agents", "アクティブエージェント" },
            { "dashboard_no_agents", "接続されているエージェントはありません" },
            
            // Device Manager
            { "devices_title", "デバイス管理" },
            { "devices_search", "デバイスを検索..." },
            { "devices_refresh", "更新" },
            { "devices_no_devices", "デバイスが見つかりません" },
            { "devices_connect", "接続" },
            { "devices_disconnect", "切断" },
            { "devices_status", "状態" },
            { "devices_name", "デバイス名" },
            { "devices_ip", "IPアドレス" },
            { "devices_os", "オペレーティングシステム" },
            { "devices_last_seen", "最終確認" },
            { "devices_actions", "アクション" },
            
            // Settings
            { "settings_title", "設定" },
            { "settings_language", "言語" },
            { "settings_language_desc", "インターフェース言語を選択" },
            { "settings_appearance", "外観" },
            { "settings_theme", "テーマ" },
            { "settings_dark_mode", "ダークモード" },
            { "settings_light_mode", "ライトモード" },
            { "settings_wallpaper", "壁紙" },
            { "settings_wallpaper_default", "デフォルト" },
            { "settings_wallpaper_sunset", "夕焼け" },
            { "settings_wallpaper_ocean", "海" },
            { "settings_wallpaper_forest", "森" },
            { "settings_wallpaper_purple", "紫" },
            { "settings_wallpaper_midnight", "深夜" },
            { "settings_notifications", "通知" },
            { "settings_push_notifications", "プッシュ通知" },
            { "settings_sound_alerts", "サウンドアラート" },
            { "settings_connection", "接続" },
            { "settings_auto_reconnect", "自動再接続" },
            { "settings_low_bandwidth", "低帯域幅モード" },
            { "settings_refresh_interval", "更新間隔" },
            { "settings_about", "について" },
            { "settings_version", "バージョン" },
            { "settings_app_name", "アプリケーション" },
            { "settings_framework", "フレームワーク" },
            
            // Login
            { "login_title", "NETSPY" },
            { "login_subtitle", "管理者認証が必要です" },
            { "login_password", "パスワード" },
            { "login_placeholder", "管理者パスワードを入力" },
            { "login_remember", "ログイン状態を保持" },
            { "login_remember_hint", "(1時間)" },
            { "login_button", "ログイン" },
            { "login_loading", "ログイン中..." },
            { "login_error_empty", "パスワードを入力してください。" },
            { "login_error_wrong", "パスワードが間違っています。" },
            { "login_error_locked", "試行回数が多すぎます。お待ちください" },
            { "login_server", "サーバー" },
            
            // Common
            { "common_save", "保存" },
            { "common_cancel", "キャンセル" },
            { "common_delete", "削除" },
            { "common_edit", "編集" },
            { "common_close", "閉じる" },
            { "common_loading", "読み込み中..." },
            { "common_error", "エラー" },
            { "common_success", "成功" },
            { "common_warning", "警告" },
            { "common_info", "情報" },
            { "common_seconds", "秒" },
            { "common_coming_soon", "近日公開" },
            
            // Device Control
            { "control_remote_screen", "リモート画面" },
            { "control_task_manager", "タスクマネージャー" },
            { "control_system_monitor", "システムモニター" },
            { "control_keylogger", "キーロガー" },
            { "control_webcam", "ウェブカメラ" },
            { "control_power", "電源" },
            { "control_registry", "レジストリ" },
            { "control_start", "開始" },
            { "control_stop", "停止" },
            { "control_capture", "キャプチャ" },
            { "control_screenshot", "スクリーンショット" }
        };

        // Chinese
        _translations["CN"] = new Dictionary<string, string>
        {
            // Navigation
            { "nav_dashboard", "仪表板" },
            { "nav_devices", "设备管理" },
            { "nav_settings", "设置" },
            
            // Dashboard
            { "dashboard_title", "仪表板" },
            { "dashboard_total", "设备总数" },
            { "dashboard_online", "在线" },
            { "dashboard_offline", "离线" },
            { "dashboard_commands", "已发送命令" },
            { "dashboard_connection_status", "连接状态" },
            { "dashboard_connected", "已连接" },
            { "dashboard_disconnected", "已断开" },
            { "dashboard_agents", "活动代理" },
            { "dashboard_no_agents", "没有连接的代理" },
            
            // Device Manager
            { "devices_title", "设备管理" },
            { "devices_search", "搜索设备..." },
            { "devices_refresh", "刷新" },
            { "devices_no_devices", "未找到设备" },
            { "devices_connect", "连接" },
            { "devices_disconnect", "断开" },
            { "devices_status", "状态" },
            { "devices_name", "设备名称" },
            { "devices_ip", "IP地址" },
            { "devices_os", "操作系统" },
            { "devices_last_seen", "最后在线" },
            { "devices_actions", "操作" },
            
            // Settings
            { "settings_title", "设置" },
            { "settings_language", "语言" },
            { "settings_language_desc", "选择界面语言" },
            { "settings_appearance", "外观" },
            { "settings_theme", "主题" },
            { "settings_dark_mode", "深色模式" },
            { "settings_light_mode", "浅色模式" },
            { "settings_wallpaper", "壁纸" },
            { "settings_wallpaper_default", "默认" },
            { "settings_wallpaper_sunset", "日落" },
            { "settings_wallpaper_ocean", "海洋" },
            { "settings_wallpaper_forest", "森林" },
            { "settings_wallpaper_purple", "紫色" },
            { "settings_wallpaper_midnight", "午夜" },
            { "settings_notifications", "通知" },
            { "settings_push_notifications", "推送通知" },
            { "settings_sound_alerts", "声音提醒" },
            { "settings_connection", "连接" },
            { "settings_auto_reconnect", "自动重连" },
            { "settings_low_bandwidth", "低带宽模式" },
            { "settings_refresh_interval", "刷新间隔" },
            { "settings_about", "关于" },
            { "settings_version", "版本" },
            { "settings_app_name", "应用程序" },
            { "settings_framework", "框架" },
            
            // Login
            { "login_title", "NETSPY" },
            { "login_subtitle", "需要管理员身份验证" },
            { "login_password", "密码" },
            { "login_placeholder", "输入管理员密码" },
            { "login_remember", "记住登录" },
            { "login_remember_hint", "(1小时)" },
            { "login_button", "登录" },
            { "login_loading", "正在登录..." },
            { "login_error_empty", "请输入密码。" },
            { "login_error_wrong", "密码错误，请重试。" },
            { "login_error_locked", "尝试次数过多，请等待" },
            { "login_server", "服务器" },
            
            // Common
            { "common_save", "保存" },
            { "common_cancel", "取消" },
            { "common_delete", "删除" },
            { "common_edit", "编辑" },
            { "common_close", "关闭" },
            { "common_loading", "加载中..." },
            { "common_error", "错误" },
            { "common_success", "成功" },
            { "common_warning", "警告" },
            { "common_info", "信息" },
            { "common_seconds", "秒" },
            { "common_coming_soon", "即将推出" },
            
            // Device Control
            { "control_remote_screen", "远程屏幕" },
            { "control_task_manager", "任务管理器" },
            { "control_system_monitor", "系统监控" },
            { "control_keylogger", "键盘记录" },
            { "control_webcam", "摄像头" },
            { "control_power", "电源" },
            { "control_registry", "注册表" },
            { "control_start", "开始" },
            { "control_stop", "停止" },
            { "control_capture", "捕获" },
            { "control_screenshot", "截图" }
        };
    }
}
