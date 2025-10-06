using System;
using System.Windows;
using System.Windows.Input;
using WpfScreenshotApp.Services;

namespace WpfScreenshotApp;

public partial class App : System.Windows.Application
{
    private TrayIconService? _trayIconService;
    private HotkeyService? _hotkeyService;
    private ScreenshotService? _screenshotService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var clipboardService = new ClipboardService();
        var stickyImageService = new StickyImageService();
        _screenshotService = new ScreenshotService(clipboardService, stickyImageService);

        _trayIconService = new TrayIconService();
        _trayIconService.Initialize("WPF Screenshot Tool", OnExitRequested, OnTakeScreenshotRequested, OnSaveScreenshotRequested);

        _hotkeyService = new HotkeyService();
        _hotkeyService.HotkeyPressed += (_, _) => StartCapture();
        if (!_hotkeyService.RegisterHotkey(ModifierKeys.Control | ModifierKeys.Alt, System.Windows.Forms.Keys.End))
        {
            System.Windows.Forms.MessageBox.Show("无法注册全局热键 Ctrl+Alt+End，可能已被其他程序占用。", "WPF Screenshot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        Dispatcher.BeginInvoke(new Action(() => _trayIconService?.ShowBalloonTip("已启动", "按 Ctrl+Alt+End 开始截图")));
    }

    private void StartCapture()
    {
        if (_screenshotService == null)
        {
            return;
        }

        Dispatcher.Invoke(() =>
        {
            _screenshotService.StartCapture();
        });
    }

    private void OnTakeScreenshotRequested()
    {
        StartCapture();
    }

    private void OnSaveScreenshotRequested()
    {
        _screenshotService?.PromptSaveLatestCapture();
    }

    private void OnExitRequested()
    {
        _hotkeyService?.Dispose();
        _trayIconService?.Dispose();
        _screenshotService?.Dispose();
        Shutdown();
    }
}
