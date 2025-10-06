using System;
using System.Windows;

namespace WpfScreenshotApp.Views;

public partial class FloatingToolbarWindow : Window
{
    public event EventHandler? PinRequested;
    public event EventHandler? EditRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? CancelRequested;

    public FloatingToolbarWindow()
    {
        InitializeComponent();
    }

    public void PlaceAt(System.Windows.Point position, Rect bounds)
    {
        UpdateLayout();
        var width = double.IsNaN(Width) || Width == 0 ? ActualWidth : Width;
        var height = double.IsNaN(Height) || Height == 0 ? ActualHeight : Height;

        var desiredLeft = position.X;
        var desiredTop = position.Y;

        if (desiredLeft + width > bounds.Right)
        {
            desiredLeft = bounds.Right - width - 10;
        }

        if (desiredTop + height > bounds.Bottom)
        {
            desiredTop = bounds.Bottom - height - 10;
        }

        if (desiredLeft < bounds.Left)
        {
            desiredLeft = bounds.Left + 10;
        }

        if (desiredTop < bounds.Top)
        {
            desiredTop = bounds.Top + 10;
        }

        Left = desiredLeft;
        Top = desiredTop;
    }

    private void OnPinClick(object sender, RoutedEventArgs e) => PinRequested?.Invoke(this, EventArgs.Empty);

    private void OnEditClick(object sender, RoutedEventArgs e) => EditRequested?.Invoke(this, EventArgs.Empty);

    private void OnSaveClick(object sender, RoutedEventArgs e) => SaveRequested?.Invoke(this, EventArgs.Empty);

    private void OnCancelClick(object sender, RoutedEventArgs e) => CancelRequested?.Invoke(this, EventArgs.Empty);
}
