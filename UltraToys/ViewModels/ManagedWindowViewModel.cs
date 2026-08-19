using CommunityToolkit.Mvvm.ComponentModel;
using UltraToys.Models;
using UltraToys.Services;

namespace UltraToys.ViewModels;

public partial class ManagedWindowViewModel : ObservableObject
{
    private readonly TopmostService _topmostService;

    public IntPtr Hwnd { get; }
    public string Title { get; }
    public string ProcessName { get; }

    [ObservableProperty]
    private bool isPinned;

    public ManagedWindowViewModel(ManagedWindowInfo info, TopmostService topmostService)
    {
        Hwnd = info.Hwnd;
        Title = info.Title;
        ProcessName = info.ProcessName;
        _topmostService = topmostService;
        isPinned = topmostService.IsPinned(info.Hwnd);
    }

    // Fires for BOTH the checkbox in the main window AND when the hotkey path updates this
    // row's IsPinned from outside — TopmostService.Pin/Unpin is idempotent, so re-invoking it
    // here for an already-matching state is harmless (SetProperty skips the no-op case anyway).
    partial void OnIsPinnedChanged(bool value)
    {
        if (value) _topmostService.Pin(Hwnd);
        else _topmostService.Unpin(Hwnd);
    }
}
