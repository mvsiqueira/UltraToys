using System.Windows;
using System.Windows.Input;
using UltraToys.ViewModels;

namespace UltraToys.Controls;

/// <summary>Button that records the next key combination the user presses as the tool's global hotkey.
/// DataContext is a <see cref="HotkeyEditorViewModel"/>.</summary>
public partial class HotkeyEditor : System.Windows.Controls.UserControl
{
    public HotkeyEditor()
    {
        InitializeComponent();
    }

    private HotkeyEditorViewModel? ViewModel => DataContext as HotkeyEditorViewModel;

    private void OnCaptureClick(object sender, RoutedEventArgs e)
    {
        ViewModel?.BeginCapture();
        CaptureButton.Focus();
    }

    private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (ViewModel is not { IsCapturing: true } viewModel) return;

        // While listening, every key belongs to the editor (no focus moves, no button activation).
        e.Handled = true;

        var key = e.Key switch
        {
            Key.System => e.SystemKey,
            Key.ImeProcessed => e.ImeProcessedKey,
            _ => e.Key,
        };

        if (key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
        {
            viewModel.CancelCapture();
            return;
        }

        // Wait for the non-modifier key that completes the combination.
        if (IsModifierKey(key) || key is Key.None or Key.DeadCharProcessed) return;

        viewModel.Apply(Keyboard.Modifiers, key);
    }

    private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        ViewModel?.CancelCapture();
    }

    private static bool IsModifierKey(Key key) =>
        key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin;
}
