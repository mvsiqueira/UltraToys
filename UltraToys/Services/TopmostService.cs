namespace UltraToys.Services;

/// <summary>
/// Single source of truth for which windows are pinned "always on top". Both the global hotkey
/// and the checkbox list in the main window call into this class, so the two trigger paths can
/// never fall out of sync — whichever fires, <see cref="PinStateChanged"/> notifies everyone.
/// </summary>
public sealed class TopmostService
{
    private readonly HashSet<IntPtr> _pinnedHwnds = new();

    public event EventHandler<IntPtr>? PinStateChanged;

    public bool IsPinned(IntPtr hwnd) => _pinnedHwnds.Contains(hwnd);

    public IReadOnlyCollection<IntPtr> PinnedWindows => _pinnedHwnds;

    public void Pin(IntPtr hwnd)
    {
        if (!NativeMethods.IsWindow(hwnd)) return;
        NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
            NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
        _pinnedHwnds.Add(hwnd);
        PinStateChanged?.Invoke(this, hwnd);
    }

    public void Unpin(IntPtr hwnd)
    {
        if (NativeMethods.IsWindow(hwnd))
        {
            NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_NOTOPMOST, 0, 0, 0, 0,
                NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
        }
        _pinnedHwnds.Remove(hwnd);
        PinStateChanged?.Invoke(this, hwnd);
    }

    public void Toggle(IntPtr hwnd)
    {
        if (IsPinned(hwnd)) Unpin(hwnd);
        else Pin(hwnd);
    }

    /// <summary>Drops hwnds for windows that have since been closed, so the set doesn't grow stale.</summary>
    public void PruneClosedWindows()
    {
        _pinnedHwnds.RemoveWhere(hwnd => !NativeMethods.IsWindow(hwnd));
    }
}
