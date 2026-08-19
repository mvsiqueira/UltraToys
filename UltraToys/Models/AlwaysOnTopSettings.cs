using System.Text.Json.Serialization;

namespace UltraToys.Models;

public class AlwaysOnTopSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("hotkey")]
    public HotkeyConfig Hotkey { get; set; } = new();

    [JsonPropertyName("show_border_overlay")]
    public bool ShowBorderOverlay { get; set; } = true;

    [JsonPropertyName("pinned_windows")]
    public List<PinnedWindowIdentity> PinnedWindows { get; set; } = new();
}
