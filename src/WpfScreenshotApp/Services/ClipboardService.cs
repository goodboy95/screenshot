using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfScreenshotApp.Services;

public class ClipboardService
{
    public void CopyImage(BitmapSource image)
    {
        Clipboard.SetImage(image);
    }
}
