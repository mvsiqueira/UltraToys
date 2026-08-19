using Microsoft.Win32;

namespace UltraToys.Services;

/// <summary>
/// Registers/unregisters UltraToys to start with Windows via the HKCU Run key (not a Startup-folder
/// shortcut). A .lnk shortcut embeds a target path at creation time and breaks silently if the user
/// moves/renames the app folder; a registry value can be cheaply re-validated and self-healed on
/// every launch since the running process always knows its own current path.
/// </summary>
public static class AutostartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "UltraToys";

    public static void Enable()
    {
        var exePath = GetCurrentExePath();
        if (exePath is null) return;

        // CreateSubKey opens the Run key if it exists or creates it if missing (unlike OpenSubKey,
        // which silently returns null for a missing key) — it never touches any other value in it.
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
        key?.SetValue(ValueName, $"\"{exePath}\"");
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    /// <summary>Called at startup — re-writes the Run value if it's stale (app moved) or removes a
    /// leftover value if autostart is currently disabled in settings.</summary>
    public static void EnsurePathCurrent(bool shouldBeEnabled)
    {
        if (shouldBeEnabled)
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }

    private static string? GetCurrentExePath() => Environment.ProcessPath;
}
