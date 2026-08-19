using System.Windows;
using System.Windows.Forms;

namespace UltraToys.Services;

/// <summary>
/// Owns the system tray icon and its context menu. Built imperatively (matches this codebase's
/// existing WinForms-interop style in UltraViagem) rather than via a WPF-native tray package,
/// since UseWindowsForms is already required for Screen.AllScreens — NotifyIcon comes for free.
/// </summary>
public sealed class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly IReadOnlyList<IUltraTool> _tools;
    private readonly Action _openMainWindow;
    private readonly Action _exitApp;
    private readonly Func<bool> _getStartWithWindows;
    private readonly Action<bool> _setStartWithWindows;

    public TrayIconService(
        IReadOnlyList<IUltraTool> tools,
        Action openMainWindow,
        Action exitApp,
        Func<bool> getStartWithWindows,
        Action<bool> setStartWithWindows)
    {
        _tools = tools;
        _openMainWindow = openMainWindow;
        _exitApp = exitApp;
        _getStartWithWindows = getStartWithWindows;
        _setStartWithWindows = setStartWithWindows;

        _notifyIcon = new NotifyIcon
        {
            Icon = LoadTrayIcon(),
            Text = "UltraToys",
            Visible = false,
        };
        _notifyIcon.MouseUp += OnTrayIconMouseUp;
    }

    public void Initialize()
    {
        RebuildMenu();
        _notifyIcon.Visible = true;
    }

    /// <summary>Rebuilds the whole context menu — call after tool state changes or monitor list changes.</summary>
    public void RebuildMenu()
    {
        var menu = new ContextMenuStrip();

        var openItem = new ToolStripMenuItem("Abrir UltraToys", null, (_, _) => _openMainWindow());
        openItem.Font = new System.Drawing.Font(openItem.Font, System.Drawing.FontStyle.Bold);
        menu.Items.Add(openItem);
        menu.Items.Add(new ToolStripSeparator());

        foreach (var tool in _tools)
        {
            tool.BuildTrayMenuItems(menu);
        }

        if (_tools.Count > 0)
        {
            menu.Items.Add(new ToolStripSeparator());
        }

        var startWithWindowsItem = new ToolStripMenuItem("Iniciar com o Windows")
        {
            CheckOnClick = true,
            Checked = _getStartWithWindows(),
        };
        startWithWindowsItem.Click += (_, _) => _setStartWithWindows(startWithWindowsItem.Checked);
        menu.Items.Add(startWithWindowsItem);

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Sair", null, (_, _) => _exitApp()));

        _notifyIcon.ContextMenuStrip = menu;
    }

    private void OnTrayIconMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _openMainWindow();
        }
    }

    private static System.Drawing.Icon LoadTrayIcon()
    {
        try
        {
            var uri = new Uri("pack://application:,,,/Resources/app-icon.ico");
            var streamInfo = System.Windows.Application.GetResourceStream(uri);
            if (streamInfo != null)
            {
                return new System.Drawing.Icon(streamInfo.Stream);
            }
        }
        catch
        {
            // Fall through to the extraction-based fallback below.
        }

        try
        {
            var path = Environment.ProcessPath;
            if (path != null)
            {
                var extracted = System.Drawing.Icon.ExtractAssociatedIcon(path);
                if (extracted != null) return extracted;
            }
        }
        catch
        {
            // Fall through to the system default below.
        }

        return System.Drawing.SystemIcons.Application;
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
