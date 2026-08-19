using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltraToys.Models;
using UltraToys.Services;

namespace UltraToys.ViewModels;

public partial class MoveToMonitorViewModel : ObservableObject
{
    private readonly MoveToMonitorTool _tool;
    private readonly MoveToMonitorSettings _settings;
    private readonly Action _saveSettings;

    public ObservableCollection<MonitorInfo> Monitors { get; } = new();

    [ObservableProperty]
    private bool isEnabled;

    public MoveToMonitorViewModel(MoveToMonitorTool tool, MoveToMonitorSettings settings, Action saveSettings)
    {
        _tool = tool;
        _settings = settings;
        _saveSettings = saveSettings;
        isEnabled = settings.Enabled;
        RefreshMonitors();
    }

    [RelayCommand]
    private void RefreshMonitors()
    {
        Monitors.Clear();
        foreach (var monitor in MonitorService.GetMonitors())
        {
            Monitors.Add(monitor);
        }
    }

    [RelayCommand]
    private void MoveAllWindowsToMonitor(MonitorInfo monitor)
    {
        _tool.MoveAllWindowsTo(monitor);
    }

    partial void OnIsEnabledChanged(bool value)
    {
        _settings.Enabled = value;
        _saveSettings();
    }
}
