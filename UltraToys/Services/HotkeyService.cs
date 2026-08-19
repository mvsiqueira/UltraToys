using System.Windows.Input;
using System.Windows.Interop;

namespace UltraToys.Services;

/// <summary>
/// WPF has no native RegisterHotKey wrapper, so this hosts a hidden message-only window
/// (HwndSource) purely to receive WM_HOTKEY — it keeps working even while MainWindow is
/// hidden/minimized to tray, since it's a separate native window from MainWindow.
/// </summary>
public sealed class HotkeyService : IDisposable
{
    private HwndSource? _source;
    private readonly Dictionary<int, Action> _handlers = new();
    private int _nextId = 0xB000;

    public void Initialize()
    {
        var parameters = new HwndSourceParameters("UltraToysHotkeyWindow")
        {
            WindowStyle = 0,
            Width = 0,
            Height = 0,
        };
        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);
    }

    /// <summary>Registers a global hotkey. Returns an id to pass to <see cref="Unregister"/>, or null if registration failed (e.g. already claimed by another app).</summary>
    public int? Register(ModifierKeys modifiers, Key key, Action callback)
    {
        if (_source is null)
            throw new InvalidOperationException("HotkeyService.Initialize() must be called first.");

        var id = _nextId++;
        var vk = (uint)KeyInterop.VirtualKeyFromKey(key);
        var mod = ToNativeModifiers(modifiers) | NativeMethods.MOD_NOREPEAT;

        if (!NativeMethods.RegisterHotKey(_source.Handle, id, mod, vk))
        {
            return null;
        }

        _handlers[id] = callback;
        return id;
    }

    public void Unregister(int id)
    {
        if (_source is null) return;
        NativeMethods.UnregisterHotKey(_source.Handle, id);
        _handlers.Remove(id);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == unchecked((int)NativeMethods.WM_HOTKEY))
        {
            var id = wParam.ToInt32();
            if (_handlers.TryGetValue(id, out var callback))
            {
                callback();
                handled = true;
            }
        }

        return IntPtr.Zero;
    }

    private static uint ToNativeModifiers(ModifierKeys modifiers)
    {
        uint result = 0;
        if (modifiers.HasFlag(ModifierKeys.Alt)) result |= NativeMethods.MOD_ALT;
        if (modifiers.HasFlag(ModifierKeys.Control)) result |= NativeMethods.MOD_CONTROL;
        if (modifiers.HasFlag(ModifierKeys.Shift)) result |= NativeMethods.MOD_SHIFT;
        if (modifiers.HasFlag(ModifierKeys.Windows)) result |= NativeMethods.MOD_WIN;
        return result;
    }

    public void Dispose()
    {
        if (_source is null) return;

        foreach (var id in _handlers.Keys.ToList())
        {
            NativeMethods.UnregisterHotKey(_source.Handle, id);
        }
        _handlers.Clear();

        _source.RemoveHook(WndProc);
        _source.Dispose();
        _source = null;
    }
}
