using System.Windows.Forms;
using UltraToys.Models;

namespace UltraToys.Services;

/// <summary>Coordinates the "Mover apps para Monitor" tool: bulk-moves open windows to a chosen
/// monitor, triggered from either the tray submenu or the main window.</summary>
public sealed class MoveToMonitorTool : IUltraTool
{
    private readonly MoveToMonitorSettings _settings;
    private readonly Action _saveSettings;
    private readonly IntPtr _ownWindowHwnd;

    public MoveToMonitorTool(MoveToMonitorSettings settings, Action saveSettings, IntPtr ownWindowHwnd)
    {
        _settings = settings;
        _saveSettings = saveSettings;
        _ownWindowHwnd = ownWindowHwnd;
    }

    public string Id => "move-to-monitor";
    public string DisplayName => "Mover apps para Monitor";
    public bool IsEnabled => _settings.Enabled;

    // No background hook/hotkey — this tool only acts on-demand.
    public void Start() { }
    public void Stop() { }

    public void MoveAllWindowsTo(MonitorInfo target)
    {
        var windows = WindowEnumerationService.GetVisibleTopLevelWindows(_ownWindowHwnd);
        WindowMoveService.MoveAllWindowsTo(target, windows);
        _settings.LastUsedDeviceName = target.DeviceName;
        _saveSettings();
    }

    public void BuildTrayMenuItems(ContextMenuStrip menu)
    {
        if (!_settings.Enabled) return;

        var submenu = new ToolStripMenuItem(DisplayName);
        foreach (var monitor in MonitorService.GetMonitors())
        {
            var label = $"Monitor {monitor.Index}" + (monitor.IsPrimary ? " (Principal)" : "") +
                        $" — {(int)monitor.Bounds.Width}x{(int)monitor.Bounds.Height}";
            var item = new ToolStripMenuItem(label);
            item.Click += (_, _) => MoveAllWindowsTo(monitor);
            submenu.DropDownItems.Add(item);
        }

        menu.Items.Add(submenu);
    }
}
