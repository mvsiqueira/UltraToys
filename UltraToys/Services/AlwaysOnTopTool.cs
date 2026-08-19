using System.Windows.Forms;
using UltraToys.Models;

namespace UltraToys.Services;

/// <summary>Coordinates the "Sempre no topo" tool: registers the global hotkey, keeps the border
/// overlay in sync with pin state, and contributes its own tray menu entry.</summary>
public sealed class AlwaysOnTopTool : IUltraTool
{
    private readonly HotkeyService _hotkeyService;
    private readonly TopmostService _topmostService;
    private readonly BorderOverlayService _overlayService;
    private readonly AlwaysOnTopSettings _settings;
    private readonly Action _saveSettings;
    private int? _hotkeyId;

    public AlwaysOnTopTool(
        HotkeyService hotkeyService,
        TopmostService topmostService,
        BorderOverlayService overlayService,
        AlwaysOnTopSettings settings,
        Action saveSettings)
    {
        _hotkeyService = hotkeyService;
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

    public void Start()
    {
        if (!_settings.Enabled || _hotkeyId is not null) return;

        if (HotkeyParser.TryParse(_settings.Hotkey, out var modifiers, out var key))
        {
            _hotkeyId = _hotkeyService.Register(modifiers, key, ToggleForegroundWindow);
        }
    }

    public void Stop()
    {
        if (_hotkeyId is int id)
        {
            _hotkeyService.Unregister(id);
            _hotkeyId = null;
        }
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
