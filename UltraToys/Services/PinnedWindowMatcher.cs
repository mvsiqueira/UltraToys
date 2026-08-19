using UltraToys.Models;

namespace UltraToys.Services;

/// <summary>
/// Best-effort restore of pinned "always on top" windows after UltraToys restarts. Window handles
/// aren't stable across process restarts, so matching is done by process name + exact title — a
/// window that hasn't opened yet at boot, or whose title changed, simply won't be re-pinned; this
/// is an accepted limitation rather than something worth chasing with fuzzier heuristics.
/// </summary>
public static class PinnedWindowMatcher
{
    public static void ReapplyPinnedWindows(
        AlwaysOnTopSettings settings,
        IReadOnlyList<ManagedWindowInfo> currentWindows,
        TopmostService topmostService)
    {
        foreach (var identity in settings.PinnedWindows)
        {
            var match = currentWindows.FirstOrDefault(w =>
                string.Equals(w.ProcessName, identity.ProcessName, StringComparison.OrdinalIgnoreCase) &&
                (identity.TitlePattern is null || string.Equals(w.Title, identity.TitlePattern, StringComparison.OrdinalIgnoreCase)));

            if (match is not null)
            {
                topmostService.Pin(match.Hwnd);
            }
        }
    }
}
