using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfScreenshotApp.Services;

public class ClipboardService
{
    public void CopyImage(BitmapSource image)
    {
        System.Windows.Clipboard.SetImage(image);
    }
}
