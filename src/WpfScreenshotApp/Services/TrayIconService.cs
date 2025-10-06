using System;
using System.Drawing;
using System.Windows.Forms;

namespace WpfScreenshotApp.Services;

public sealed class TrayIconService : IDisposable
{
    private NotifyIcon? _notifyIcon;
    private Action? _exitAction;
    private Action? _captureAction;
    private Action? _saveAction;

    public void Initialize(string tooltipText, Action exitAction, Action captureAction, Action saveAction)
    {
        _exitAction = exitAction;
        _captureAction = captureAction;
        _saveAction = saveAction;

        _notifyIcon = new NotifyIcon
        {
            Text = tooltipText,
            Visible = true,
            Icon = SystemIcons.Application
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("截图 (&S)", null, (_, _) => _captureAction?.Invoke());
        contextMenu.Items.Add("保存最新截图 (&A)", null, (_, _) => _saveAction?.Invoke());
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("退出 (&E)", null, (_, _) => _exitAction?.Invoke());

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (_, _) => _captureAction?.Invoke();
    }

    public void ShowBalloonTip(string title, string message)
    {
        if (_notifyIcon == null)
        {
            return;
        }

        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.ShowBalloonTip(3000);
    }

    public void Dispose()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
    }
}
