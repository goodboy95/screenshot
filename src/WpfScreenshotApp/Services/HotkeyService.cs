using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using WpfScreenshotApp.Interop;
using ModifierKeys = System.Windows.Input.ModifierKeys;

namespace WpfScreenshotApp.Services;

public sealed class HotkeyService : IDisposable
{
    private readonly int _hotkeyId;
    private HwndSource? _source;
    private Window? _messageWindow;
    private bool _registered;

    public event EventHandler? HotkeyPressed;

    public HotkeyService()
    {
        _hotkeyId = GetHashCode();
    }

    public bool RegisterHotkey(ModifierKeys modifiers, Keys key)
    {
        if (_registered)
        {
            return true;
        }

        _messageWindow = new Window
        {
            Visibility = Visibility.Hidden,
            ShowInTaskbar = false,
            Width = 0,
            Height = 0,
            WindowStyle = WindowStyle.None
        };

        _messageWindow.Show();
        _messageWindow.Hide();

        var helper = new WindowInteropHelper(_messageWindow);
        _source = HwndSource.FromHwnd(helper.Handle);
        if (_source == null)
        {
            _messageWindow.Close();
            _messageWindow = null;
            return false;
        }

        _source.AddHook(WndProc);
        var mods = ConvertModifiers(modifiers);
        _registered = NativeMethods.RegisterHotKey(_source.Handle, _hotkeyId, (uint)mods, (uint)key);
        if (!_registered)
        {
            _source.RemoveHook(WndProc);
            _messageWindow?.Close();
            _messageWindow = null;
        }

        return _registered;
    }

    public void UnregisterHotkey()
    {
        if (_registered && _source != null)
        {
            NativeMethods.UnregisterHotKey(_source.Handle, _hotkeyId);
            _source.RemoveHook(WndProc);
            _source.Dispose();
            _source = null;
            _registered = false;
        }

        if (_messageWindow != null)
        {
            _messageWindow.Close();
            _messageWindow = null;
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
        {
            handled = true;
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }

        return IntPtr.Zero;
    }

    private static NativeMethods.HotKeyModifiers ConvertModifiers(ModifierKeys modifiers)
    {
        var result = NativeMethods.HotKeyModifiers.None;

        if (modifiers.HasFlag(ModifierKeys.Alt))
        {
            result |= NativeMethods.HotKeyModifiers.Alt;
        }

        if (modifiers.HasFlag(ModifierKeys.Control))
        {
            result |= NativeMethods.HotKeyModifiers.Control;
        }

        if (modifiers.HasFlag(ModifierKeys.Shift))
        {
            result |= NativeMethods.HotKeyModifiers.Shift;
        }

        if (modifiers.HasFlag(ModifierKeys.Windows))
        {
            result |= NativeMethods.HotKeyModifiers.Win;
        }

        return result;
    }

    public void Dispose()
    {
        UnregisterHotkey();
    }
}
