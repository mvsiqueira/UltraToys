using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltraToys.Models;
using UltraToys.Services;

namespace UltraToys.ViewModels;

public partial class AlwaysOnTopViewModel : ObservableObject
{
    private readonly AlwaysOnTopTool _tool;
    private readonly TopmostService _topmostService;
    private readonly BorderOverlayService _overlayService;
    private readonly AlwaysOnTopSettings _settings;
    private readonly Action _saveSettings;
    private readonly IntPtr _ownWindowHwnd;

    public ObservableCollection<ManagedWindowViewModel> Windows { get; } = new();

    [ObservableProperty]
    private bool isEnabled;

    [ObservableProperty]
    private bool showBorderOverlay;

    public string HotkeyDisplay => HotkeyParser.Format(_settings.Hotkey);

    public AlwaysOnTopViewModel(
        AlwaysOnTopTool tool,
        TopmostService topmostService,
        BorderOverlayService overlayService,
        AlwaysOnTopSettings settings,
        Action saveSettings,
        IntPtr ownWindowHwnd)
    {
        _tool = tool;
        _topmostService = topmostService;
        _overlayService = overlayService;
        _settings = settings;
        _saveSettings = saveSettings;
        _ownWindowHwnd = ownWindowHwnd;

        isEnabled = settings.Enabled;
        showBorderOverlay = settings.ShowBorderOverlay;

        _topmostService.PinStateChanged += OnPinStateChanged;
        RefreshWindows();
    }

    [RelayCommand]
    private void RefreshWindows()
    {
        _topmostService.PruneClosedWindows();
        var current = WindowEnumerationService.GetVisibleTopLevelWindows(_ownWindowHwnd);

        Windows.Clear();
        foreach (var info in current)
        {
            Windows.Add(new ManagedWindowViewModel(info, _topmostService));
        }
    }

    partial void OnIsEnabledChanged(bool value)
    {
        _settings.Enabled = value;
        if (value) _tool.Start(); else _tool.Stop();
        _saveSettings();
    }

    partial void OnShowBorderOverlayChanged(bool value)
    {
        _settings.ShowBorderOverlay = value;
        _overlayService.Enabled = value;
        if (!value) _overlayService.HideAll();
        _saveSettings();
    }

    private void OnPinStateChanged(object? sender, IntPtr hwnd)
    {
        var isPinned = _topmostService.IsPinned(hwnd);

        var row = Windows.FirstOrDefault(w => w.Hwnd == hwnd);
        if (row is not null)
        {
            row.IsPinned = isPinned;
        }

        // The pinned window may not be in the displayed list (e.g. pinned via hotkey after the last
        // refresh), so fall back to a fresh lookup to get its process name and title.
        var info = row is not null
            ? new ManagedWindowInfo { Hwnd = hwnd, Title = row.Title, ProcessName = row.ProcessName }
            : WindowEnumerationService.GetVisibleTopLevelWindows(_ownWindowHwnd).FirstOrDefault(w => w.Hwnd == hwnd);

        if (info is not null)
        {
            UpdatePersistedPin(info, isPinned);
        }
    }

    // Keeps the persisted pinned-window list (process name + title) in sync incrementally: pinning
    // adds the window, unpinning removes only the identities that match it. Saved identities for
    // windows that aren't open right now are left untouched, so they can be re-pinned on a later
    // start (PinnedWindowMatcher) instead of being forgotten.
    private void UpdatePersistedPin(ManagedWindowInfo info, bool isPinned)
    {
        var list = _settings.PinnedWindows;
        var changed = false;

        if (isPinned)
        {
            if (!list.Any(identity => PinnedWindowMatcher.Matches(identity, info)))
            {
                list.Add(new PinnedWindowIdentity { ProcessName = info.ProcessName, TitlePattern = info.Title });
                changed = true;
            }
        }
        else
        {
            changed = list.RemoveAll(identity => PinnedWindowMatcher.Matches(identity, info)) > 0;
        }

        if (changed)
        {
            _saveSettings();
        }
    }
}
