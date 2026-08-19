using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using UltraToys.Models;
using UltraToys.Services;
using UltraToys.ViewModels;
using UltraToys.Views;

namespace UltraToys;

public partial class App : System.Windows.Application
{
    [DllImport("shell32.dll")]
    private static extern void SetCurrentProcessExplicitAppUserModelID(
        [MarshalAs(UnmanagedType.LPWStr)] string appId);

    private AppSettings _settings = new();
    private TrayIconService? _trayIconService;
    private HotkeyService? _hotkeyService;
    private BorderOverlayService? _overlayService;
    private List<IUltraTool> _tools = new();
    private bool _isExiting;

    protected override void OnStartup(StartupEventArgs e)
    {
        SetCurrentProcessExplicitAppUserModelID("Ultrasoft.UltraToys");
        base.OnStartup(e);

        _settings = PersistenceService.LoadSettings();
        AutostartService.EnsurePathCurrent(_settings.StartWithWindows);

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Closing += OnMainWindowClosing;
        var ownHwnd = new WindowInteropHelper(mainWindow).EnsureHandle();

        void SaveSettings() => PersistenceService.SaveSettings(_settings);

        _hotkeyService = new HotkeyService();
        _hotkeyService.Initialize();

        var topmostService = new TopmostService();
        _overlayService = new BorderOverlayService();

        var alwaysOnTopTool = new AlwaysOnTopTool(
            _hotkeyService, topmostService, _overlayService, _settings.AlwaysOnTop, SaveSettings);

        var alwaysOnTopViewModel = new AlwaysOnTopViewModel(
            alwaysOnTopTool, topmostService, _overlayService, _settings.AlwaysOnTop, SaveSettings, ownHwnd);

        PinnedWindowMatcher.ReapplyPinnedWindows(
            _settings.AlwaysOnTop, WindowEnumerationService.GetVisibleTopLevelWindows(ownHwnd), topmostService);

        var moveToMonitorTool = new MoveToMonitorTool(_settings.MoveToMonitor, SaveSettings, ownHwnd);
        var moveToMonitorViewModel = new MoveToMonitorViewModel(moveToMonitorTool, _settings.MoveToMonitor, SaveSettings);

        _tools = new List<IUltraTool> { alwaysOnTopTool, moveToMonitorTool };
        foreach (var tool in _tools)
        {
            if (tool.IsEnabled) tool.Start();
        }

        var mainViewModel = new MainViewModel(
            _settings.StartWithWindows, alwaysOnTopViewModel, moveToMonitorViewModel, ApplyStartWithWindows);
        mainWindow.AttachViewModel(mainViewModel);

        _trayIconService = new TrayIconService(
            _tools,
            openMainWindow: () => ShowMainWindow(mainWindow),
            exitApp: ExitApplication,
            getStartWithWindows: () => _settings.StartWithWindows,
            setStartWithWindows: enabled =>
            {
                ApplyStartWithWindows(enabled);
                mainViewModel.StartWithWindows = enabled;
            });
        _trayIconService.Initialize();

        mainWindow.Show();
    }

    private static void ShowMainWindow(Window window)
    {
        window.Show();
        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }
        window.Activate();
    }

    private void OnMainWindowClosing(object? sender, CancelEventArgs e)
    {
        if (_isExiting) return;

        // Tray-resident behavior: closing the window just hides it, the process keeps running.
        e.Cancel = true;
        (sender as Window)?.Hide();
    }

    private void ApplyStartWithWindows(bool enabled)
    {
        _settings.StartWithWindows = enabled;
        AutostartService.EnsurePathCurrent(enabled);
        PersistenceService.SaveSettings(_settings);
    }

    private void ExitApplication()
    {
        _isExiting = true;

        foreach (var tool in _tools)
        {
            tool.Stop();
        }

        PersistenceService.SaveSettings(_settings);
        _trayIconService?.Dispose();
        _overlayService?.Dispose();
        _hotkeyService?.Dispose();
        Shutdown();
    }
}
