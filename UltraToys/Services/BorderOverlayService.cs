using System.Windows.Threading;
using UltraToys.Views;

namespace UltraToys.Services;

/// <summary>Shows/hides a click-through colored border around each pinned "always on top" window,
/// following its position with a lightweight poll timer.</summary>
public sealed class BorderOverlayService : IDisposable
{
    private readonly Dictionary<IntPtr, OverlayWindow> _overlays = new();
    private readonly DispatcherTimer _timer;

    public BorderOverlayService()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _timer.Tick += (_, _) => UpdateAllPositions();
    }

    public bool Enabled { get; set; } = true;

    public void ShowFor(IntPtr hwnd)
    {
        if (!Enabled || _overlays.ContainsKey(hwnd)) return;

        var overlay = new OverlayWindow();
        overlay.Show();
        _overlays[hwnd] = overlay;
        UpdatePosition(hwnd, overlay);

        if (!_timer.IsEnabled) _timer.Start();
    }

    public void HideFor(IntPtr hwnd)
    {
        if (_overlays.Remove(hwnd, out var overlay))
        {
            overlay.Close();
        }

        if (_overlays.Count == 0) _timer.Stop();
    }

    public void HideAll()
    {
        foreach (var overlay in _overlays.Values) overlay.Close();
        _overlays.Clear();
        _timer.Stop();
    }

    private void UpdateAllPositions()
    {
        foreach (var (hwnd, overlay) in _overlays.ToList())
        {
            if (!NativeMethods.IsWindow(hwnd) || !NativeMethods.IsWindowVisible(hwnd))
            {
                HideFor(hwnd);
                continue;
            }

            UpdatePosition(hwnd, overlay);
        }
    }

    private static void UpdatePosition(IntPtr hwnd, OverlayWindow overlay)
    {
        if (!NativeMethods.GetWindowRect(hwnd, out var rect)) return;

        overlay.Left = rect.Left;
        overlay.Top = rect.Top;
        overlay.Width = Math.Max(0, rect.Right - rect.Left);
        overlay.Height = Math.Max(0, rect.Bottom - rect.Top);
    }

    public void Dispose() => HideAll();
}
