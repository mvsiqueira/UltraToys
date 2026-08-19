using System.Windows.Input;
using UltraToys.Models;

namespace UltraToys.Services;

/// <summary>Converts between the serializable <see cref="HotkeyConfig"/> and WPF's ModifierKeys/Key.</summary>
public static class HotkeyParser
{
    public static bool TryParse(HotkeyConfig config, out ModifierKeys modifiers, out Key key)
    {
        modifiers = ModifierKeys.None;
        key = Key.None;

        foreach (var part in config.Modifiers.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            modifiers |= part.ToLowerInvariant() switch
            {
                "win" or "windows" => ModifierKeys.Windows,
                "ctrl" or "control" => ModifierKeys.Control,
                "alt" => ModifierKeys.Alt,
                "shift" => ModifierKeys.Shift,
                _ => ModifierKeys.None,
            };
        }

        if (!Enum.TryParse(config.Key, ignoreCase: true, out key))
        {
            return false;
        }

        return modifiers != ModifierKeys.None && key != Key.None;
    }

    public static string Format(HotkeyConfig config) => $"{config.Modifiers} + {config.Key}".Replace("+", " + ");
}
