using System.Text.RegularExpressions;
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

    /// <summary>Canonical modifier text for persistence, always in the order Win, Ctrl, Alt, Shift.</summary>
    public static string FormatModifiers(ModifierKeys modifiers)
    {
        var parts = new List<string>();
        if (modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
        if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        return string.Join("+", parts);
    }

    /// <summary>Human-readable form, e.g. "Win + Ctrl + T".</summary>
    public static string Format(string modifiers, string key)
    {
        var parts = modifiers.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Append(FormatKey(key));
        return string.Join(" + ", parts);
    }

    public static string Format(HotkeyConfig config) => Format(config.Modifiers, config.Key);

    // WPF names the digit row keys D0..D9; show them as the digit itself.
    private static string FormatKey(string key) =>
        Regex.IsMatch(key, @"^D\d$") ? key[1..] : key;
}
