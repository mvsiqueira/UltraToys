using System.Windows;

namespace UltraToys.Models;

public class MonitorInfo
{
    public required string DeviceName { get; init; }
    public required int Index { get; init; }
    public required Rect Bounds { get; init; }
    public required Rect WorkingArea { get; init; }
    public required bool IsPrimary { get; init; }
}
