using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace MachineLabel;

public partial class MainWindow : Window
{
    private LabelSettings _settings = null!;
    private DispatcherTimer _positionTimer = null!;
    private bool _isDragging;
    private Point _dragStart;
    private double _dpiScale = 1.0;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Get DPI scale
        var source = PresentationSource.FromVisual(this);
        if (source?.CompositionTarget != null)
            _dpiScale = source.CompositionTarget.TransformToDevice.M11;

        // Make the window a tool window (no alt-tab entry)
        MakeToolWindow();

        _settings = LabelSettings.Load();
        mnuStartup.IsChecked = _settings.StartWithWindows;
        ApplySettings();

        // Timer to keep label positioned on the taskbar
        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _positionTimer.Tick += PositionTimer_Tick;
        _positionTimer.Start();

        PositionOnTaskbar();
    }

    private void MakeToolWindow()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        int exStyle = TaskbarHelper.GetWindowLong(hwnd, TaskbarHelper.GWL_EXSTYLE);
        exStyle |= TaskbarHelper.WS_EX_TOOLWINDOW;
        exStyle |= TaskbarHelper.WS_EX_NOACTIVATE;
        TaskbarHelper.SetWindowLong(hwnd, TaskbarHelper.GWL_EXSTYLE, exStyle);
    }

    public void ApplySettings()
    {
        try
        {
            LabelText.Text = _settings.LabelText;
            LabelText.FontSize = _settings.FontSize;
            LabelText.FontWeight = _settings.Bold ? FontWeights.Bold : FontWeights.Normal;
            LabelText.Foreground = ParseBrush(_settings.TextColor, "#FFFFFF");
            LabelBorder.Background = ParseBrush(_settings.BackgroundColor, "#FF6B35");
            LabelBorder.CornerRadius = new CornerRadius(_settings.CornerRadius);
            LabelBorder.Padding = new Thickness(_settings.PaddingH, _settings.PaddingV,
                _settings.PaddingH, _settings.PaddingV);
            Opacity = _settings.Opacity;

            // Defer position recalc so layout has time to measure new text
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
            {
                PositionOnTaskbar();
            });
        }
        catch { }
    }

    private static SolidColorBrush ParseBrush(string colorStr, string fallback)
    {
        try
        {
            var obj = ColorConverter.ConvertFromString(colorStr);
            if (obj is Color c) return new SolidColorBrush(c);
        }
        catch { }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(fallback));
    }

    private void PositionTimer_Tick(object? sender, EventArgs e)
    {
        if (_isDragging || _settings == null) return;

        try
        {
            bool visible = TaskbarHelper.IsTaskbarVisible();
            if (visible)
            {
                if (Visibility != Visibility.Visible)
                    Visibility = Visibility.Visible;
                PositionOnTaskbar();
            }
            else
            {
                Visibility = Visibility.Hidden;
            }
        }
        catch { }
    }

    private void PositionOnTaskbar()
    {
        if (_settings == null || ActualWidth == 0 || ActualHeight == 0) return;

        var (taskbarRect, edge) = TaskbarHelper.GetTaskbarInfo();

        double tbLeft = taskbarRect.Left / _dpiScale;
        double tbTop = taskbarRect.Top / _dpiScale;
        double tbWidth = (taskbarRect.Right - taskbarRect.Left) / _dpiScale;
        double tbHeight = (taskbarRect.Bottom - taskbarRect.Top) / _dpiScale;

        if (tbWidth <= 0 || tbHeight <= 0) return;

        double offsetX = _settings.OffsetX / _dpiScale;

        switch (edge)
        {
            case TaskbarHelper.TaskbarEdge.Bottom:
                // Place label in the center area of the taskbar, shifted by offset
                Left = tbLeft + (tbWidth / 2) - (ActualWidth / 2) + offsetX;
                Top = tbTop + (tbHeight - ActualHeight) / 2;
                break;

            case TaskbarHelper.TaskbarEdge.Top:
                Left = tbLeft + (tbWidth / 2) - (ActualWidth / 2) + offsetX;
                Top = tbTop + (tbHeight - ActualHeight) / 2;
                break;

            case TaskbarHelper.TaskbarEdge.Left:
                Left = tbLeft + (tbWidth - ActualWidth) / 2;
                Top = tbTop + (tbHeight / 2) - (ActualHeight / 2) + offsetX;
                break;

            case TaskbarHelper.TaskbarEdge.Right:
                Left = tbLeft + (tbWidth - ActualWidth) / 2;
                Top = tbTop + (tbHeight / 2) - (ActualHeight / 2) + offsetX;
                break;
        }
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            OpenSettings();
            return;
        }

        _isDragging = true;
        _dragStart = e.GetPosition(this);
        LabelBorder.CaptureMouse();
        LabelBorder.MouseMove += Border_MouseMove;
        LabelBorder.MouseLeftButtonUp += Border_MouseLeftButtonUp;
    }

    private void Border_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;

        var (_, edge) = TaskbarHelper.GetTaskbarInfo();
        var currentPos = e.GetPosition(this);
        double delta;

        if (edge == TaskbarHelper.TaskbarEdge.Left || edge == TaskbarHelper.TaskbarEdge.Right)
            delta = (currentPos.Y - _dragStart.Y) * _dpiScale;
        else
            delta = (currentPos.X - _dragStart.X) * _dpiScale;

        _settings.OffsetX += (int)delta;
        PositionOnTaskbar();
    }

    private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        LabelBorder.ReleaseMouseCapture();
        LabelBorder.MouseMove -= Border_MouseMove;
        LabelBorder.MouseLeftButtonUp -= Border_MouseLeftButtonUp;
        _settings.Save();
    }

    private void Settings_Click(object sender, RoutedEventArgs e) => OpenSettings();

    private void OpenSettings()
    {
        // Pause timer while settings dialog is open
        _positionTimer?.Stop();

        try
        {
            var settingsWindow = new SettingsWindow(_settings);
            // Don't set Owner=this — MainWindow is a tool window with WS_EX_NOACTIVATE
            // which causes reactivation crashes when the dialog closes

            if (settingsWindow.ShowDialog() == true)
            {
                _settings = settingsWindow.ResultSettings;
                _settings.Save();
                ApplySettings();
            }
        }
        finally
        {
            _positionTimer?.Start();
        }
    }

    private void CopyName_Click(object sender, RoutedEventArgs e)
    {
        try { Clipboard.SetText(Environment.MachineName); } catch { }
    }

    private void Startup_Click(object sender, RoutedEventArgs e)
    {
        _settings.SetStartWithWindows(mnuStartup.IsChecked);
        _settings.Save();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _positionTimer?.Stop();
        Application.Current.Shutdown();
    }

    protected override void OnClosed(EventArgs e)
    {
        _positionTimer?.Stop();
        base.OnClosed(e);
    }
}
