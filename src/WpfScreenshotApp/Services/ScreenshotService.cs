using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using WpfScreenshotApp.Models;
using WpfScreenshotApp.Views;
using Application = System.Windows.Application;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;

namespace WpfScreenshotApp.Services;

public class ScreenshotService : IDisposable
{
    private readonly ClipboardService _clipboardService;
    private readonly StickyImageService _stickyImageService;
    private ScreenshotResult? _latestResult;
    private OverlayWindow? _overlayWindow;
    private FloatingToolbarWindow? _toolbarWindow;
    private Bitmap? _fullScreenshot;
    private Rect _virtualBounds;

    public ScreenshotService(ClipboardService clipboardService, StickyImageService stickyImageService)
    {
        _clipboardService = clipboardService;
        _stickyImageService = stickyImageService;
    }

    public void StartCapture()
    {
        if (_overlayWindow != null)
        {
            return;
        }

        _virtualBounds = GetVirtualScreenBounds();
        _fullScreenshot = CaptureVirtualScreen(_virtualBounds);
        var preview = ConvertToBitmapSource(_fullScreenshot);

        _overlayWindow = new OverlayWindow(preview, _virtualBounds);
        _overlayWindow.SelectionCompleted += OnSelectionCompleted;
        _overlayWindow.SelectionCanceled += OnSelectionCanceled;
        _overlayWindow.Closed += (_, _) => ClearOverlay();
        _overlayWindow.Show();
        _overlayWindow.Activate();
    }

    public void PromptSaveLatestCapture()
    {
        if (_latestResult == null)
        {
            System.Windows.MessageBox.Show("没有可以保存的截图。", "WPF Screenshot", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SaveScreenshot(_latestResult);
    }

    private void OnSelectionCompleted(object? sender, SelectionCompletedEventArgs e)
    {
        if (_overlayWindow == null || _fullScreenshot == null)
        {
            return;
        }

        var rect = e.Selection;
        var cropRect = new Rectangle(
            (int)Math.Round(rect.X - _virtualBounds.Left),
            (int)Math.Round(rect.Y - _virtualBounds.Top),
            (int)Math.Round(rect.Width),
            (int)Math.Round(rect.Height));

        cropRect.Intersect(new Rectangle(0, 0, _fullScreenshot.Width, _fullScreenshot.Height));
        if (cropRect.Width <= 0 || cropRect.Height <= 0)
        {
            return;
        }

        using var croppedBitmap = _fullScreenshot.Clone(cropRect, PixelFormat.Format32bppArgb);
        var bitmapSource = ConvertToBitmapSource(croppedBitmap);
        _latestResult = new ScreenshotResult(bitmapSource, rect);

        Application.Current.Dispatcher.Invoke(() =>
        {
            _clipboardService.CopyImage(bitmapSource);
        });

        ShowToolbar(e.ReleasePoint);
    }

    private void OnSelectionCanceled(object? sender, EventArgs e)
    {
        _overlayWindow?.Close();
    }

    private void ShowToolbar(Point releasePoint)
    {
        CloseToolbar();
        if (_latestResult == null)
        {
            return;
        }

        _toolbarWindow = new FloatingToolbarWindow
        {
            Owner = _overlayWindow
        };
        _toolbarWindow.PinRequested += (_, _) => PinLatest();
        _toolbarWindow.EditRequested += (_, _) => EditLatest();
        _toolbarWindow.SaveRequested += (_, _) => SaveLatest();
        _toolbarWindow.CancelRequested += (_, _) => CancelLatest();
        _toolbarWindow.Loaded += (_, _) =>
        {
            var anchorPoint = new Point(releasePoint.X + 12, releasePoint.Y + 12);
            _toolbarWindow.PlaceAt(anchorPoint, _virtualBounds);
        };
        _toolbarWindow.Closed += (_, _) => _toolbarWindow = null;
        _toolbarWindow.Show();
    }

    private void PinLatest()
    {
        if (_latestResult == null)
        {
            return;
        }

        _stickyImageService.ShowStickyImage(_latestResult.Image);
        CloseToolbar();
        _overlayWindow?.Close();
    }

    private void EditLatest()
    {
        if (_latestResult == null)
        {
            return;
        }

        var tempPath = Path.Combine(Path.GetTempPath(), $"screenshot-{DateTime.Now:yyyyMMddHHmmssfff}.png");
        SaveBitmapSourceToFile(_latestResult.Image, tempPath);

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "mspaint.exe",
                Arguments = $"\"{tempPath}\"",
                UseShellExecute = true
            });

            if (process != null)
            {
                Task.Run(() =>
                {
                    process.WaitForExit();
                    TryDeleteFile(tempPath);
                });
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"无法打开画图程序: {ex.Message}", "WPF Screenshot", MessageBoxButton.OK, MessageBoxImage.Error);
            TryDeleteFile(tempPath);
        }

        CloseToolbar();
        _overlayWindow?.Close();
    }

    private void SaveLatest()
    {
        if (_latestResult == null)
        {
            return;
        }

        SaveScreenshot(_latestResult);
        CloseToolbar();
        _overlayWindow?.Close();
    }

    private void CancelLatest()
    {
        CloseToolbar();
        _overlayWindow?.Close();
    }

    private void SaveScreenshot(ScreenshotResult result)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg",
            FileName = $"screenshot-{result.CapturedAt:yyyyMMddHHmmss}"
        };

        if (dialog.ShowDialog() == true)
        {
            SaveBitmapSourceToFile(result.Image, dialog.FileName);
            result.SavedFilePath = dialog.FileName;
            System.Windows.MessageBox.Show($"已保存到 {dialog.FileName}", "WPF Screenshot", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private static void SaveBitmapSourceToFile(BitmapSource source, string filePath)
    {
        BitmapEncoder encoder = Path.GetExtension(filePath).Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            ? new JpegBitmapEncoder()
            : new PngBitmapEncoder();

        encoder.Frames.Add(BitmapFrame.Create(source));
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        encoder.Save(fileStream);
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Ignore
        }
    }

    private static Rect GetVirtualScreenBounds()
    {
        return new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);
    }

    private static Bitmap CaptureVirtualScreen(Rect bounds)
    {
        var width = (int)Math.Round(bounds.Width);
        var height = (int)Math.Round(bounds.Height);
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen((int)Math.Round(bounds.Left), (int)Math.Round(bounds.Top), 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
        return bitmap;
    }

    private static BitmapSource ConvertToBitmapSource(Bitmap bitmap)
    {
        var hBitmap = bitmap.GetHbitmap();
        try
        {
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            NativeMethodsWrapper.DeleteObject(hBitmap);
        }
    }

    private void ClearOverlay()
    {
        CloseToolbar();
        _overlayWindow = null;
        _fullScreenshot?.Dispose();
        _fullScreenshot = null;
    }

    private void CloseToolbar()
    {
        if (_toolbarWindow != null)
        {
            _toolbarWindow.Close();
            _toolbarWindow = null;
        }
    }

    public void Dispose()
    {
        CloseToolbar();
        _stickyImageService.CloseAll();
        ClearOverlay();
    }

    private static class NativeMethodsWrapper
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
