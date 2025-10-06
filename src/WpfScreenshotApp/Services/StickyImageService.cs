using System.Collections.Generic;
using System.Windows.Media.Imaging;
using WpfScreenshotApp.Views;

namespace WpfScreenshotApp.Services;

public class StickyImageService
{
    private readonly List<StickyImageWindow> _windows = new();

    public void ShowStickyImage(BitmapSource image)
    {
        var window = new StickyImageWindow(image);
        window.Closed += (_, _) => _windows.Remove(window);
        _windows.Add(window);
        window.Show();
    }

    public void CloseAll()
    {
        foreach (var window in _windows.ToArray())
        {
            window.Close();
        }

        _windows.Clear();
    }
}
