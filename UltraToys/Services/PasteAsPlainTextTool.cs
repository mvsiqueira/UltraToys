using System.Runtime.InteropServices;
using System.Windows.Forms;
using UltraToys.Models;

namespace UltraToys.Services;

/// <summary>Coordinates the "Colar sem formatação" tool: on the global hotkey it replaces the
/// clipboard content with its plain-text version and pastes it into the focused app (Ctrl+V).</summary>
public sealed class PasteAsPlainTextTool : IUltraTool
{
    private readonly PasteAsPlainTextSettings _settings;
    private readonly Action _saveSettings;
    private bool _busy;

    public PasteAsPlainTextTool(HotkeyService hotkeyService, PasteAsPlainTextSettings settings, Action saveSettings)
    {
        Hotkey = new GlobalHotkey(DisplayName, hotkeyService, settings.Hotkey, OnHotkey);
        _settings = settings;
        _saveSettings = saveSettings;
    }

    public string Id => "paste-as-plain-text";
    public string DisplayName => "Colar sem formatação";
    public bool IsEnabled => _settings.Enabled;

    /// <summary>True while the global hotkey is registered. False when the tool is off or the
    /// hotkey is already claimed by another app.</summary>
    public bool IsHotkeyRegistered => Hotkey.IsRegistered;

    public GlobalHotkey Hotkey { get; }

    public void Start()
    {
        if (!_settings.Enabled) return;
        Hotkey.Register();
    }

    public void Stop() => Hotkey.Unregister();

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

    private async void OnHotkey()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            await PasteAsPlainTextAsync();
        }
        catch
        {
            // A failed paste must never take the tray app down.
        }
        finally
        {
            _busy = false;
        }
    }

    private static async Task PasteAsPlainTextAsync()
    {
        // The hotkey fires while Win+Ctrl+V are still held down. Sending Ctrl+V now would reach the
        // target as Win+Ctrl+V (and even re-trigger this hotkey), so wait until the user lets go.
        if (!await WaitForKeysReleasedAsync(TimeSpan.FromSeconds(2))) return;

        // Keep the clipboard as it is when there is no text to flatten (image, files...): the
        // hotkey swallowed the user's keystroke, so give them a normal paste instead.
        var text = await ReadClipboardTextAsync();
        if (!string.IsNullOrEmpty(text))
        {
            await WriteClipboardTextAsync(text);
        }

        SendCtrlV();
    }

    private static async Task<bool> WaitForKeysReleasedAsync(TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (AnyHotkeyKeyDown())
        {
            if (DateTime.UtcNow > deadline) return false;
            await Task.Delay(10);
        }
        return true;
    }

    private static bool AnyHotkeyKeyDown()
    {
        foreach (var vk in new int[]
                 {
                     NativeMethods.VK_LWIN, NativeMethods.VK_RWIN, NativeMethods.VK_CONTROL,
                     NativeMethods.VK_SHIFT, NativeMethods.VK_MENU, NativeMethods.VK_V,
                 })
        {
            if ((NativeMethods.GetAsyncKeyState(vk) & 0x8000) != 0) return true;
        }
        return false;
    }

    // The clipboard can be briefly locked by whichever app just copied; retry a few times.
    private static async Task<string?> ReadClipboardTextAsync()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                return System.Windows.Clipboard.ContainsText(System.Windows.TextDataFormat.UnicodeText)
                    ? System.Windows.Clipboard.GetText(System.Windows.TextDataFormat.UnicodeText)
                    : null;
            }
            catch (COMException)
            {
                await Task.Delay(30);
            }
        }
        return null;
    }

    private static async Task WriteClipboardTextAsync(string text)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                // copy: true keeps the data on the clipboard even after this process exits.
                System.Windows.Clipboard.SetDataObject(
                    new System.Windows.DataObject(System.Windows.DataFormats.UnicodeText, text), copy: true);
                return;
            }
            catch (COMException)
            {
                await Task.Delay(30);
            }
        }
    }

    private static void SendCtrlV()
    {
        var inputs = new[]
        {
            Key(NativeMethods.VK_CONTROL, keyUp: false),
            Key(NativeMethods.VK_V, keyUp: false),
            Key(NativeMethods.VK_V, keyUp: true),
            Key(NativeMethods.VK_CONTROL, keyUp: true),
        };
        NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    private static INPUT Key(ushort vk, bool keyUp) => new()
    {
        type = NativeMethods.INPUT_KEYBOARD,
        U = new INPUTUNION
        {
            ki = new KEYBDINPUT { wVk = vk, dwFlags = keyUp ? NativeMethods.KEYEVENTF_KEYUP : 0 },
        },
    };
}
