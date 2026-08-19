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
        var row = Windows.FirstOrDefault(w => w.Hwnd == hwnd);
        if (row is not null)
        {
            row.IsPinned = _topmostService.IsPinned(hwnd);
        }

        PersistPinnedWindows();
    }

    // Rebuilds the persisted pinned-window list (by process name + title) from the hwnds
    // TopmostService currently tracks, so a restart can best-effort re-pin them (PinnedWindowMatcher).
    private void PersistPinnedWindows()
    {
        _settings.PinnedWindows.Clear();
        foreach (var pinnedHwnd in _topmostService.PinnedWindows)
        {
            var row = Windows.FirstOrDefault(w => w.Hwnd == pinnedHwnd);
            if (row is not null)
            {
                _settings.PinnedWindows.Add(new PinnedWindowIdentity
                {
                    ProcessName = row.ProcessName,
                    TitlePattern = row.Title,
                });
            }
        }

        _saveSettings();
    }
}
