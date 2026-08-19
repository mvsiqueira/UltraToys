using CommunityToolkit.Mvvm.ComponentModel;

namespace UltraToys.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Action<bool> _onStartWithWindowsChanged;

    public AlwaysOnTopViewModel AlwaysOnTop { get; }
    public MoveToMonitorViewModel MoveToMonitor { get; }

    [ObservableProperty]
    private bool startWithWindows;

    public MainViewModel(
        bool startWithWindows,
        AlwaysOnTopViewModel alwaysOnTop,
        MoveToMonitorViewModel moveToMonitor,
        Action<bool> onStartWithWindowsChanged)
    {
        _onStartWithWindowsChanged = onStartWithWindowsChanged;
        AlwaysOnTop = alwaysOnTop;
        MoveToMonitor = moveToMonitor;
        this.startWithWindows = startWithWindows;
    }

    partial void OnStartWithWindowsChanged(bool value) => _onStartWithWindowsChanged(value);
}
