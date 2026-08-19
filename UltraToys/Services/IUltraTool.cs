using System.Windows.Forms;

namespace UltraToys.Services;

/// <summary>
/// Shared contract for a single UltraToys tool. Not a plugin system — tools are compiled into the
/// same exe and registered by hand in App.xaml.cs; this interface just gives the tray icon and the
/// app lifecycle a single place to hook each tool's startup/shutdown and menu contribution, so
/// adding tool #3 later doesn't mean touching TrayIconService or App.xaml.cs by hand for wiring.
/// </summary>
public interface IUltraTool
{
    string Id { get; }
    string DisplayName { get; }
    bool IsEnabled { get; }

    /// <summary>Registers hotkeys/hooks. Called once at startup if IsEnabled, and again if the user re-enables the tool.</summary>
    void Start();

    /// <summary>Unregisters hotkeys/hooks. Called on app exit, or when the user disables the tool.</summary>
    void Stop();

    /// <summary>Appends this tool's own items to the tray context menu.</summary>
    void BuildTrayMenuItems(ContextMenuStrip menu);
}
