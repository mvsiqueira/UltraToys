using System.Diagnostics;
using System.Text;
using UltraToys.Models;

namespace UltraToys.Services;

/// <summary>Enumerates top-level, user-visible application windows via user32.dll (EnumWindows).</summary>
public static class WindowEnumerationService
{
    /// <summary>
    /// Returns the visible top-level windows a user would consider "an open app" — excludes
    /// tool windows, owned windows (dialogs/popups), windows with no title, and (optionally)
    /// the caller's own window.
    /// </summary>
    public static IReadOnlyList<ManagedWindowInfo> GetVisibleTopLevelWindows(IntPtr excludeHwnd = default)
    {
        var results = new List<ManagedWindowInfo>();

        NativeMethods.EnumWindows((hWnd, _) =>
        {
            if (hWnd == excludeHwnd) return true;
            if (!NativeMethods.IsWindowVisible(hWnd)) return true;

            // Skip owned windows (dialogs, tool popups) — real top-level apps have no owner.
            if (NativeMethods.GetWindow(hWnd, NativeMethods.GW_OWNER) != IntPtr.Zero) return true;

            var exStyle = (long)NativeMethods.GetWindowLongPtr(hWnd, NativeMethods.GWL_EXSTYLE);
            if ((exStyle & NativeMethods.WS_EX_TOOLWINDOW) != 0 &&
                (exStyle & NativeMethods.WS_EX_APPWINDOW) == 0)
            {
                return true;
            }

            var title = GetWindowTitle(hWnd);
            if (string.IsNullOrWhiteSpace(title)) return true;

            NativeMethods.GetWindowThreadProcessId(hWnd, out var pid);
            string processName;
            try
            {
                processName = Process.GetProcessById((int)pid).ProcessName;
            }
            catch
            {
                processName = string.Empty;
            }

            results.Add(new ManagedWindowInfo
            {
                Hwnd = hWnd,
                Title = title,
                ProcessName = processName,
            });

            return true;
        }, IntPtr.Zero);

        return results;
    }

    private static string GetWindowTitle(IntPtr hWnd)
    {
        var length = NativeMethods.GetWindowTextLength(hWnd);
        if (length == 0) return string.Empty;

        var sb = new StringBuilder(length + 1);
        NativeMethods.GetWindowText(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }
}
