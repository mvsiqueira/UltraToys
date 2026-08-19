using System.Text.Json.Serialization;

namespace UltraToys.Models;

/// <summary>
/// Best-effort identity used to re-pin a window after UltraToys restarts (window handles are not
/// stable across process restarts, so matching is done by process name + optional title pattern).
/// </summary>
public class PinnedWindowIdentity
{
    [JsonPropertyName("process_name")]
    public string ProcessName { get; set; } = string.Empty;

    [JsonPropertyName("title_pattern")]
    public string? TitlePattern { get; set; }
}
