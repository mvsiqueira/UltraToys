using System.Text.Json.Serialization;

namespace UltraToys.Models;

public class PasteAsPlainTextSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("hotkey")]
    public HotkeyConfig Hotkey { get; set; } = new() { Modifiers = "Win+Ctrl", Key = "V" };
}
