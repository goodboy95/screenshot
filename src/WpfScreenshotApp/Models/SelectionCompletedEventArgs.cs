using System;
using System.Windows;
using Point = System.Windows.Point;

namespace WpfScreenshotApp.Models;

public sealed class SelectionCompletedEventArgs : EventArgs
{
    public SelectionCompletedEventArgs(Rect selection, Point releasePoint)
    {
        Selection = selection;
        ReleasePoint = releasePoint;
    }

    public Rect Selection { get; }

    public Point ReleasePoint { get; }
}
