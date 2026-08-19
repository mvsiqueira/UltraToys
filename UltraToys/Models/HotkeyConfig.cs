using System.Text.Json.Serialization;

namespace UltraToys.Models;

/// <summary>Serializable global-hotkey definition, e.g. "Win+Ctrl" + "T".</summary>
public class HotkeyConfig
{
    [JsonPropertyName("modifiers")]
    public string Modifiers { get; set; } = "Win+Ctrl";

    [JsonPropertyName("key")]
    public string Key { get; set; } = "T";
}
