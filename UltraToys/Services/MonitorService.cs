using System.Windows;
using UltraToys.Models;
using Forms = System.Windows.Forms;

namespace UltraToys.Services;

/// <summary>Enumerates monitors via WinForms' Screen.AllScreens (same pattern as UltraViagem).</summary>
public static class MonitorService
{
    public static IReadOnlyList<MonitorInfo> GetMonitors()
    {
        var screens = Forms.Screen.AllScreens.OrderBy(s => s.Bounds.Left).ToList();

        return screens.Select((screen, i) => new MonitorInfo
        {
            DeviceName = screen.DeviceName,
            Index = i + 1,
            Bounds = new Rect(screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height),
            WorkingArea = new Rect(screen.WorkingArea.Left, screen.WorkingArea.Top, screen.WorkingArea.Width, screen.WorkingArea.Height),
            IsPrimary = screen.Primary,
        }).ToList();
    }
}
