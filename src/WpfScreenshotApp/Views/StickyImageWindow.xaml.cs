using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WpfScreenshotApp.Views;

public partial class StickyImageWindow : Window
{
    public StickyImageWindow(BitmapSource image)
    {
        InitializeComponent();
        StickyImage.Source = image;
        var dpiX = image.DpiX <= 0 ? 96d : image.DpiX;
        var dpiY = image.DpiY <= 0 ? 96d : image.DpiY;
        StickyImage.Width = image.PixelWidth * 96d / dpiX;
        StickyImage.Height = image.PixelHeight * 96d / dpiY;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        Close();
    }
}
