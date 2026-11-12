using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltraToys.Models;
using UltraToys.Services;

namespace UltraToys.ViewModels;

/// <summary>Drives the "press the new shortcut" editor of one global hotkey.</summary>
public partial class HotkeyEditorViewModel : ObservableObject
{
    private readonly GlobalHotkey _hotkey;
    private readonly HotkeyConfig _default;
    private readonly Func<bool> _isToolEnabled;
    private readonly Action _saveSettings;
    private readonly Action _onChanged;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Display))]
    private bool isCapturing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? error;

    public HotkeyEditorViewModel(
        GlobalHotkey hotkey,
        HotkeyConfig defaultConfig,
        Func<bool> isToolEnabled,
        Action saveSettings,
        Action onChanged)
    {
        _hotkey = hotkey;
        _default = defaultConfig;
        _isToolEnabled = isToolEnabled;
        _saveSettings = saveSettings;
        _onChanged = onChanged;
    }

    public string Display => IsCapturing
        ? "Pressione o novo atalho…"
        : HotkeyParser.Format(_hotkey.Config);

    public bool HasError => !string.IsNullOrEmpty(Error);

    public bool CanReset => !IsCurrent(_default.Modifiers, _default.Key);

    /// <summary>Starts listening for a key combination. All of the app's hotkeys are released meanwhile,
    /// otherwise pressing a combination already in use would fire that tool instead of reaching the editor.</summary>
    public void BeginCapture()
    {
        if (IsCapturing) return;
        _hotkey.BeginCapture();
        Error = null;
        IsCapturing = true;
    }

    public void CancelCapture()
    {
        if (!IsCapturing) return;
        IsCapturing = false;
        Restore();
    }

    /// <summary>Tries to bind the pressed combination. Invalid combinations keep the editor listening.</summary>
    public void Apply(ModifierKeys modifiers, Key key)
    {
        if (modifiers == ModifierKeys.None || modifiers == ModifierKeys.Shift)
        {
            Error = "Use Ctrl, Alt ou Win (sozinho ou junto com Shift).";
            return;
        }

        IsCapturing = false;
        Change(HotkeyParser.FormatModifiers(modifiers), key.ToString());
    }

    [RelayCommand]
    private void ResetToDefault()
    {
        if (IsCapturing) CancelCapture();
        Change(_default.Modifiers, _default.Key);
    }

    private void Change(string modifiers, string key)
    {
        if (IsCurrent(modifiers, key))
        {
            Error = null;
            Restore();
            return;
        }

        var previous = HotkeyParser.Format(_hotkey.Config);
        var wanted = HotkeyParser.Format(modifiers, key);

        if (_hotkey.FindConflictingTool(modifiers, key) is { } otherTool)
        {
            Error = $"{wanted} já é usado por \"{otherTool}\". Mantido: {previous}.";
        }
        else if (_hotkey.TryChange(modifiers, key, _isToolEnabled()))
        {
            Error = null;
            _saveSettings();
        }
        else
        {
            Error = $"{wanted} já está em uso por outro aplicativo. Mantido: {previous}.";
        }

        Restore();
    }

    // Puts the app's hotkeys back after a capture (with or without a change).
    private void Restore()
    {
        _hotkey.EndCapture();
        Refresh();
    }

    private void Refresh()
    {
        OnPropertyChanged(nameof(Display));
        OnPropertyChanged(nameof(CanReset));
        _onChanged();
    }

    private bool IsCurrent(string modifiers, string key) =>
        string.Equals(_hotkey.Config.Modifiers, modifiers, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(_hotkey.Config.Key, key, StringComparison.OrdinalIgnoreCase);
}
