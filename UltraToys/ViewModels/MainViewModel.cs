using CommunityToolkit.Mvvm.ComponentModel;

namespace UltraToys.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Action<bool> _onStartWithWindowsChanged;
    private readonly Action<bool> _onStartInTrayChanged;

    public AlwaysOnTopViewModel AlwaysOnTop { get; }
    public MoveToMonitorViewModel MoveToMonitor { get; }
    public PasteAsPlainTextViewModel PasteAsPlainText { get; }

    [ObservableProperty]
    private bool startWithWindows;

    [ObservableProperty]
    private bool startInTray;

    public MainViewModel(
        bool startWithWindows,
        bool startInTray,
        AlwaysOnTopViewModel alwaysOnTop,
        MoveToMonitorViewModel moveToMonitor,
        PasteAsPlainTextViewModel pasteAsPlainText,
        Action<bool> onStartWithWindowsChanged,
        Action<bool> onStartInTrayChanged)
    {
        _onStartWithWindowsChanged = onStartWithWindowsChanged;
        _onStartInTrayChanged = onStartInTrayChanged;
        AlwaysOnTop = alwaysOnTop;
        MoveToMonitor = moveToMonitor;
        PasteAsPlainText = pasteAsPlainText;
        this.startWithWindows = startWithWindows;
        this.startInTray = startInTray;
    }

    partial void OnStartWithWindowsChanged(bool value) => _onStartWithWindowsChanged(value);

    partial void OnStartInTrayChanged(bool value) => _onStartInTrayChanged(value);
}
