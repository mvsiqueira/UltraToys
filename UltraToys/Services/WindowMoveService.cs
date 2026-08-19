using System.Windows;
using UltraToys.Models;

namespace UltraToys.Services;

/// <summary>Bulk-moves open windows to a chosen monitor, handling maximized/minimized windows.</summary>
public static class WindowMoveService
{
    public static void MoveAllWindowsTo(MonitorInfo target, IReadOnlyList<ManagedWindowInfo>? windows = null)
    {
        var monitors = MonitorService.GetMonitors();
        var list = windows ?? WindowEnumerationService.GetVisibleTopLevelWindows();

        var cascadeOffset = 0;
        foreach (var window in list)
        {
            MoveWindow(window.Hwnd, target, monitors, cascadeOffset);
            cascadeOffset += 24;
        }
    }

    private static void MoveWindow(IntPtr hwnd, MonitorInfo target, IReadOnlyList<MonitorInfo> monitors, int cascadeOffset)
    {
        if (!NativeMethods.IsWindow(hwnd)) return;

        if (NativeMethods.IsIconic(hwnd))
        {
            RepositionMinimizedWindow(hwnd, target, monitors);
            return;
        }

        if (!NativeMethods.GetWindowRect(hwnd, out var initialRect)) return;
        var currentBounds = ToRect(initialRect);

        // Window already visually on the target monitor — leave it alone (covers maximized too,
        // since a maximized window's rect roughly matches its current monitor's bounds).
        if (target.Bounds.Contains(CenterOf(currentBounds))) return;

        var wasMaximized = NativeMethods.IsZoomed(hwnd);
        if (wasMaximized)
        {
            // SetWindowPos alone does not relocate a maximized window across monitors — it must
            // be restored first, moved, then re-maximized on the new monitor.
            NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);
            if (NativeMethods.GetWindowRect(hwnd, out var restoredRect))
            {
                currentBounds = ToRect(restoredRect);
            }
        }

        var sourceMonitor = FindMonitorContaining(monitors, currentBounds) ?? target;
        var (newX, newY) = ComputeTargetPosition(currentBounds, sourceMonitor, target, cascadeOffset);

        NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, newX, newY, 0, 0,
            NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE);

        if (wasMaximized)
        {
            NativeMethods.ShowWindow(hwnd, NativeMethods.SW_MAXIMIZE);
        }
    }

    private static void RepositionMinimizedWindow(IntPtr hwnd, MonitorInfo target, IReadOnlyList<MonitorInfo> monitors)
    {
        var placement = new WINDOWPLACEMENT { length = System.Runtime.InteropServices.Marshal.SizeOf<WINDOWPLACEMENT>() };
        if (!NativeMethods.GetWindowPlacement(hwnd, ref placement)) return;

        var normalBounds = ToRect(placement.rcNormalPosition);
        if (target.Bounds.Contains(CenterOf(normalBounds))) return; // restore position already on target

        var sourceMonitor = FindMonitorContaining(monitors, normalBounds) ?? target;
        var (newX, newY) = ComputeTargetPosition(normalBounds, sourceMonitor, target, cascadeOffset: 0);

        var width = normalBounds.Width;
        var height = normalBounds.Height;
        placement.rcNormalPosition = new RECT
        {
            Left = newX,
            Top = newY,
            Right = newX + (int)width,
            Bottom = newY + (int)height,
        };

        // showCmd is left untouched — the window stays minimized, but its restore position
        // (where it lands when the user un-minimizes it) is now on the target monitor.
        NativeMethods.SetWindowPlacement(hwnd, ref placement);
    }

    private static (int X, int Y) ComputeTargetPosition(Rect current, MonitorInfo source, MonitorInfo target, int cascadeOffset)
    {
        var fracX = source.WorkingArea.Width > 0 ? (current.Left - source.WorkingArea.Left) / source.WorkingArea.Width : 0;
        var fracY = source.WorkingArea.Height > 0 ? (current.Top - source.WorkingArea.Top) / source.WorkingArea.Height : 0;

        var x = target.WorkingArea.Left + fracX * target.WorkingArea.Width + cascadeOffset;
        var y = target.WorkingArea.Top + fracY * target.WorkingArea.Height + cascadeOffset;

        // Keep the window fully inside the target work area when it fits; if the window itself
        // is larger than the target monitor, fall back to aligning it to the top-left.
        var maxX = Math.Max(target.WorkingArea.Left, target.WorkingArea.Right - current.Width);
        var maxY = Math.Max(target.WorkingArea.Top, target.WorkingArea.Bottom - current.Height);
        x = Math.Clamp(x, target.WorkingArea.Left, maxX);
        y = Math.Clamp(y, target.WorkingArea.Top, maxY);

        return ((int)x, (int)y);
    }

    private static MonitorInfo? FindMonitorContaining(IReadOnlyList<MonitorInfo> monitors, Rect bounds)
    {
        var center = CenterOf(bounds);
        return monitors.FirstOrDefault(m => m.Bounds.Contains(center));
    }

    private static System.Windows.Point CenterOf(Rect rect) => new(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);

    private static Rect ToRect(RECT rect) => new(rect.Left, rect.Top, Math.Max(0, rect.Right - rect.Left), Math.Max(0, rect.Bottom - rect.Top));
}
