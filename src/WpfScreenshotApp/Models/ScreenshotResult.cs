using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfScreenshotApp.Models;

public class ScreenshotResult
{
    public ScreenshotResult(BitmapSource image, Rect area)
    {
        Image = image;
        Area = area;
        CapturedAt = DateTime.Now;
    }

    public BitmapSource Image { get; }

    public Rect Area { get; }

    public DateTime CapturedAt { get; }

    public string? SavedFilePath { get; set; }
}
