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
