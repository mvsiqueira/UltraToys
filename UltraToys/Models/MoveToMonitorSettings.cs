using System.Text.Json.Serialization;

namespace UltraToys.Models;

public class MoveToMonitorSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>Persisted MonitorInfo.DeviceName of the last monitor the user moved windows to.</summary>
    [JsonPropertyName("last_used_device_name")]
    public string? LastUsedDeviceName { get; set; }
}
