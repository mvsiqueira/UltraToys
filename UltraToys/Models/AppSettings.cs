using System.Text.Json.Serialization;

namespace UltraToys.Models;

public class AppSettings
{
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "dark";

    [JsonPropertyName("start_with_windows")]
    public bool StartWithWindows { get; set; } = false;

    [JsonPropertyName("window_width")]
    public double WindowWidth { get; set; } = 700;

    [JsonPropertyName("window_height")]
    public double WindowHeight { get; set; } = 560;

    [JsonPropertyName("window_left")]
    public double? WindowLeft { get; set; }

    [JsonPropertyName("window_top")]
    public double? WindowTop { get; set; }

    [JsonPropertyName("always_on_top")]
    public AlwaysOnTopSettings AlwaysOnTop { get; set; } = new();

    [JsonPropertyName("move_to_monitor")]
    public MoveToMonitorSettings MoveToMonitor { get; set; } = new();
}
