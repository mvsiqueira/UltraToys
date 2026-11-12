using CommunityToolkit.Mvvm.ComponentModel;
using UltraToys.Models;
using UltraToys.Services;

namespace UltraToys.ViewModels;

public partial class PasteAsPlainTextViewModel : ObservableObject
{
    private readonly PasteAsPlainTextTool _tool;
    private readonly PasteAsPlainTextSettings _settings;
    private readonly Action _saveSettings;

    [ObservableProperty]
    private bool isEnabled;

    public HotkeyEditorViewModel Hotkey { get; }

    /// <summary>True when the tool is on but its hotkey could not be registered (taken by another app).</summary>
    public bool HotkeyConflict => IsEnabled && !_tool.IsHotkeyRegistered;

    public PasteAsPlainTextViewModel(PasteAsPlainTextTool tool, PasteAsPlainTextSettings settings, Action saveSettings)
    {
        _tool = tool;
        _settings = settings;
        _saveSettings = saveSettings;
        isEnabled = settings.Enabled;

        Hotkey = new HotkeyEditorViewModel(
            tool.Hotkey,
            new HotkeyConfig { Modifiers = "Win+Ctrl", Key = "V" },
            () => _settings.Enabled,
            saveSettings,
            () => OnPropertyChanged(nameof(HotkeyConflict)));
    }

    partial void OnIsEnabledChanged(bool value)
    {
        _settings.Enabled = value;
        if (value) _tool.Start(); else _tool.Stop();
        _saveSettings();
        OnPropertyChanged(nameof(HotkeyConflict));
    }
}
