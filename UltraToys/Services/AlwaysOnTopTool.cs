using System.Windows.Forms;
using UltraToys.Models;

namespace UltraToys.Services;

/// <summary>Coordinates the "Sempre no topo" tool: registers the global hotkey, keeps the border
/// overlay in sync with pin state, and contributes its own tray menu entry.</summary>
public sealed class AlwaysOnTopTool : IUltraTool
{
    private readonly TopmostService _topmostService;
    private readonly BorderOverlayService _overlayService;
    private readonly AlwaysOnTopSettings _settings;
    private readonly Action _saveSettings;

    public AlwaysOnTopTool(
        HotkeyService hotkeyService,
        TopmostService topmostService,
        BorderOverlayService overlayService,
        AlwaysOnTopSettings settings,
        Action saveSettings)
    {
        Hotkey = new GlobalHotkey(DisplayName, hotkeyService, settings.Hotkey, ToggleForegroundWindow);
        _topmostService = topmostService;
        _overlayService = overlayService;
        _settings = settings;
        _saveSettings = saveSettings;
        _overlayService.Enabled = settings.ShowBorderOverlay;

        _topmostService.PinStateChanged += OnPinStateChanged;
    }

    public string Id => "always-on-top";
    public string DisplayName => "Sempre no topo";
    public bool IsEnabled => _settings.Enabled;

    public GlobalHotkey Hotkey { get; }

    /// <summary>True while the global hotkey is registered. False when the tool is off or the
    /// hotkey is already claimed by another app.</summary>
    public bool IsHotkeyRegistered => Hotkey.IsRegistered;

    public void Start()
    {
        if (!_settings.Enabled) return;
        Hotkey.Register();
    }

    public void Stop()
    {
        Hotkey.Unregister();
        _overlayService.HideAll();
    }

    public void BuildTrayMenuItems(ContextMenuStrip menu)
    {
        var item = new ToolStripMenuItem(DisplayName) { CheckOnClick = true, Checked = _settings.Enabled };
        item.Click += (_, _) =>
        {
            _settings.Enabled = item.Checked;
            if (_settings.Enabled) Start(); else Stop();
            _saveSettings();
        };
        menu.Items.Add(item);
    }

    private void ToggleForegroundWindow()
    {
        var hwnd = NativeMethods.GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return;
        _topmostService.Toggle(hwnd);
    }

    private void OnPinStateChanged(object? sender, IntPtr hwnd)
    {
        if (_topmostService.IsPinned(hwnd))
        {
            _overlayService.ShowFor(hwnd);
        }
        else
        {
            _overlayService.HideFor(hwnd);
        }
    }
}
