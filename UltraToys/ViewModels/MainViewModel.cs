using CommunityToolkit.Mvvm.ComponentModel;

namespace UltraToys.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Action<bool> _onStartWithWindowsChanged;
    private readonly Action<bool> _onStartInTrayChanged;

    public AlwaysOnTopViewModel AlwaysOnTop { get; }
    public MoveToMonitorViewModel MoveToMonitor { get; }

    [ObservableProperty]
    private bool startWithWindows;

    [ObservableProperty]
    private bool startInTray;

    public MainViewModel(
        bool startWithWindows,
        bool startInTray,
        AlwaysOnTopViewModel alwaysOnTop,
        MoveToMonitorViewModel moveToMonitor,
        Action<bool> onStartWithWindowsChanged,
        Action<bool> onStartInTrayChanged)
    {
        _onStartWithWindowsChanged = onStartWithWindowsChanged;
        _onStartInTrayChanged = onStartInTrayChanged;
        AlwaysOnTop = alwaysOnTop;
        MoveToMonitor = moveToMonitor;
        this.startWithWindows = startWithWindows;
        this.startInTray = startInTray;
    }

    partial void OnStartWithWindowsChanged(bool value) => _onStartWithWindowsChanged(value);

    partial void OnStartInTrayChanged(bool value) => _onStartInTrayChanged(value);
}
