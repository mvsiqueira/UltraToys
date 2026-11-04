using UltraToys.Models;

namespace UltraToys.Services;

/// <summary>
/// Best-effort restore of pinned "always on top" windows after UltraToys restarts. Window handles
/// aren't stable across process restarts, so matching is done by process name + exact title (a null
/// title matches any window of that process) — a window that hasn't opened yet at boot, or whose
/// title changed, simply won't be re-pinned, but its saved identity is kept for the next start.
/// </summary>
public static class PinnedWindowMatcher
{
    public static bool Matches(PinnedWindowIdentity identity, ManagedWindowInfo window) =>
        string.Equals(window.ProcessName, identity.ProcessName, StringComparison.OrdinalIgnoreCase) &&
        (identity.TitlePattern is null ||
         string.Equals(window.Title, identity.TitlePattern, StringComparison.OrdinalIgnoreCase));

    public static void ReapplyPinnedWindows(
        AlwaysOnTopSettings settings,
        IReadOnlyList<ManagedWindowInfo> currentWindows,
        TopmostService topmostService)
    {
        // Iterate a snapshot: each Pin() raises PinStateChanged, whose handler may edit
        // settings.PinnedWindows — enumerating the live list would throw "Collection was modified".
        foreach (var identity in settings.PinnedWindows.ToList())
        {
            var match = currentWindows.FirstOrDefault(w => Matches(identity, w));
            if (match is not null)
            {
                topmostService.Pin(match.Hwnd);
            }
        }
    }
}
