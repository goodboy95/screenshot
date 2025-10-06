using System;
using System.Runtime.InteropServices;

namespace WpfScreenshotApp.Interop;

internal static class NativeMethods
{
    public const int WM_HOTKEY = 0x0312;

    [Flags]
    public enum HotKeyModifiers : uint
    {
        None = 0,
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Win = 0x0008
    }

    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
