using UltraToys.Models;

namespace UltraToys.Services;

/// <summary>One configurable global hotkey: owns its <see cref="HotkeyConfig"/> (the instance stored in
/// the settings), its registration with <see cref="HotkeyService"/>, and safe re-binding to a new combination.</summary>
public sealed class GlobalHotkey
{
    private readonly HotkeyService _service;
    private readonly Action _callback;
    private int? _id;
    private bool _resumeAfterCapture;

    public GlobalHotkey(string name, HotkeyService service, HotkeyConfig config, Action callback)
    {
        Name = name;
        _service = service;
        Config = config;
        _callback = callback;
        service.Track(this);
    }

    /// <summary>Name of the tool that owns this hotkey (used in conflict messages).</summary>
    public string Name { get; }

    public HotkeyConfig Config { get; }

    public bool IsRegistered => _id is not null;

    /// <summary>Registers the current combination. Returns false if it could not be registered
    /// (already claimed by another app, or an invalid combination).</summary>
    public bool Register()
    {
        if (_id is not null) return true;
        if (!HotkeyParser.TryParse(Config, out var modifiers, out var key)) return false;

        _id = _service.Register(modifiers, key, _callback);
        return _id is not null;
    }

    public void Unregister()
    {
        if (_id is int id)
        {
            _service.Unregister(id);
            _id = null;
        }
    }

    /// <summary>Releases every hotkey of the app so the next key combination reaches the editor instead of
    /// firing a tool. <see cref="EndCapture"/> puts them back.</summary>
    public void BeginCapture() => _service.SuspendAll();

    public void EndCapture() => _service.ResumeAll();

    /// <summary>Name of another tool already using this combination, or null.</summary>
    public string? FindConflictingTool(string modifiers, string key) =>
        _service.FindOther(this, modifiers, key)?.Name;

    internal void Suspend()
    {
        _resumeAfterCapture = IsRegistered;
        Unregister();
    }

    internal void Resume()
    {
        if (_resumeAfterCapture) Register();
        _resumeAfterCapture = false;
    }

    /// <summary>Switches to a new combination, but only if the system accepts it. On failure the previous
    /// combination is restored (and re-registered if <paramref name="keepRegistered"/>). When the tool is off
    /// (<paramref name="keepRegistered"/> false) the new combination is only test-registered and released.</summary>
    public bool TryChange(string modifiers, string key, bool keepRegistered)
    {
        var oldModifiers = Config.Modifiers;
        var oldKey = Config.Key;

        Unregister();
        Config.Modifiers = modifiers;
        Config.Key = key;

        if (Register())
        {
            if (!keepRegistered) Unregister();
            return true;
        }

        Config.Modifiers = oldModifiers;
        Config.Key = oldKey;
        if (keepRegistered) Register();
        return false;
    }
}
