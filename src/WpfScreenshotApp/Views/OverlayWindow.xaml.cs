using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfScreenshotApp.Models;

namespace WpfScreenshotApp.Views;

public partial class OverlayWindow : Window
{
    private System.Windows.Point? _startPoint;
    private bool _selectionFinalized;
    private readonly Rect _virtualBounds;

    public event EventHandler<SelectionCompletedEventArgs>? SelectionCompleted;
    public event EventHandler? SelectionCanceled;

    public OverlayWindow(BitmapSource screenshot, Rect virtualBounds)
    {
        InitializeComponent();
        _virtualBounds = virtualBounds;
        ScreenshotImage.Source = screenshot;
        Width = virtualBounds.Width;
        Height = virtualBounds.Height;
        Left = virtualBounds.Left;
        Top = virtualBounds.Top;
        Cursor = System.Windows.Input.Cursors.Cross;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        Focus();
        Keyboard.Focus(this);
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_selectionFinalized)
        {
            return;
        }

        _startPoint = e.GetPosition(SelectionCanvas);
        Canvas.SetLeft(SelectionBorder, _startPoint.Value.X);
        Canvas.SetTop(SelectionBorder, _startPoint.Value.Y);
        SelectionBorder.Width = 0;
        SelectionBorder.Height = 0;
        SelectionBorder.Visibility = Visibility.Visible;
        CaptureMouse();
    }

    private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_selectionFinalized || _startPoint == null || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var currentPoint = e.GetPosition(SelectionCanvas);
        var x = Math.Min(currentPoint.X, _startPoint.Value.X);
        var y = Math.Min(currentPoint.Y, _startPoint.Value.Y);
        var width = Math.Abs(currentPoint.X - _startPoint.Value.X);
        var height = Math.Abs(currentPoint.Y - _startPoint.Value.Y);

        Canvas.SetLeft(SelectionBorder, x);
        Canvas.SetTop(SelectionBorder, y);
        SelectionBorder.Width = width;
        SelectionBorder.Height = height;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_selectionFinalized || _startPoint == null)
        {
            return;
        }

        ReleaseMouseCapture();
        var currentPoint = e.GetPosition(SelectionCanvas);
        var x = Math.Min(currentPoint.X, _startPoint.Value.X);
        var y = Math.Min(currentPoint.Y, _startPoint.Value.Y);
        var width = Math.Abs(currentPoint.X - _startPoint.Value.X);
        var height = Math.Abs(currentPoint.Y - _startPoint.Value.Y);
        _startPoint = null;

        if (width < 5 || height < 5)
        {
            SelectionBorder.Visibility = Visibility.Collapsed;
            return;
        }

        _selectionFinalized = true;
        Cursor = System.Windows.Input.Cursors.Arrow;

        var selection = new Rect(x + _virtualBounds.Left, y + _virtualBounds.Top, width, height);
        var releasePoint = new System.Windows.Point(currentPoint.X + _virtualBounds.Left, currentPoint.Y + _virtualBounds.Top);
        SelectionCompleted?.Invoke(this, new SelectionCompletedEventArgs(selection, releasePoint));
    }

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            SelectionCanceled?.Invoke(this, EventArgs.Empty);
        }
    }
}
