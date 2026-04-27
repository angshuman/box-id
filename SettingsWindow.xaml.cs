using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MachineLabel;

public partial class SettingsWindow : Window
{
    public LabelSettings ResultSettings { get; private set; }
    private bool _isLoading = true;

    public SettingsWindow(LabelSettings settings)
    {
        InitializeComponent();
        ResultSettings = new LabelSettings
        {
            LabelText = settings.LabelText,
            BackgroundColor = settings.BackgroundColor,
            TextColor = settings.TextColor,
            FontSize = settings.FontSize,
            Bold = settings.Bold,
            OffsetX = settings.OffsetX,
            StartWithWindows = settings.StartWithWindows,
            Opacity = settings.Opacity,
            CornerRadius = settings.CornerRadius,
            PaddingH = settings.PaddingH,
            PaddingV = settings.PaddingV,
        };

        txtLabelText.Text = settings.LabelText;
        txtBgColor.Text = settings.BackgroundColor;
        txtFgColor.Text = settings.TextColor;
        sliderFontSize.Value = settings.FontSize;
        chkBold.IsChecked = settings.Bold;
        sliderOpacity.Value = settings.Opacity;
        sliderCorner.Value = settings.CornerRadius;

        _isLoading = false;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_isLoading) return;
        try
        {
            var bgObj = ColorConverter.ConvertFromString(txtBgColor.Text);
            var fgObj = ColorConverter.ConvertFromString(txtFgColor.Text);
            if (bgObj is not Color bgColor || fgObj is not Color fgColor) return;

            previewBorder.Background = new SolidColorBrush(bgColor);
            previewBorder.CornerRadius = new CornerRadius(sliderCorner.Value);
            previewBorder.Opacity = sliderOpacity.Value;

            previewText.Text = txtLabelText.Text;
            previewText.Foreground = new SolidColorBrush(fgColor);
            previewText.FontSize = sliderFontSize.Value;
            previewText.FontWeight = chkBold.IsChecked == true ? FontWeights.Bold : FontWeights.Normal;

            bgPreview.Background = new SolidColorBrush(bgColor);
            fgPreview.Background = new SolidColorBrush(fgColor);

            lblFontSize.Text = $"{sliderFontSize.Value:F0} px";
            lblOpacity.Text = $"{sliderOpacity.Value:P0}";
            lblCorner.Text = $"{sliderCorner.Value:F0} px";
        }
        catch { }
    }

    private void Color_Changed(object sender, TextChangedEventArgs e) => UpdatePreview();
    private void Slider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdatePreview();

    private void Swatch_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string color)
        {
            txtBgColor.Text = color;
            UpdatePreview();
        }
    }

    private void Preset_MachineName(object sender, RoutedEventArgs e)
    {
        txtLabelText.Text = $"🖥️ {Environment.MachineName}";
        txtBgColor.Text = "#FF6B35";
        txtFgColor.Text = "#FFFFFF";
        UpdatePreview();
    }

    private void Preset_Prod(object sender, RoutedEventArgs e)
    {
        txtLabelText.Text = $"🔴 PROD - {Environment.MachineName}";
        txtBgColor.Text = "#F38BA8";
        txtFgColor.Text = "#1E1E2E";
        UpdatePreview();
    }

    private void Preset_Staging(object sender, RoutedEventArgs e)
    {
        txtLabelText.Text = $"🟡 STAGING - {Environment.MachineName}";
        txtBgColor.Text = "#F9E2AF";
        txtFgColor.Text = "#1E1E2E";
        UpdatePreview();
    }

    private void Preset_Dev(object sender, RoutedEventArgs e)
    {
        txtLabelText.Text = $"🟢 DEV - {Environment.MachineName}";
        txtBgColor.Text = "#A6E3A1";
        txtFgColor.Text = "#1E1E2E";
        UpdatePreview();
    }

    private void Preset_Test(object sender, RoutedEventArgs e)
    {
        txtLabelText.Text = $"🔵 TEST - {Environment.MachineName}";
        txtBgColor.Text = "#89B4FA";
        txtFgColor.Text = "#1E1E2E";
        UpdatePreview();
    }

    private void ResetPosition_Click(object sender, RoutedEventArgs e)
    {
        ResultSettings.OffsetX = 0;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        ResultSettings.LabelText = txtLabelText.Text;
        ResultSettings.BackgroundColor = txtBgColor.Text;
        ResultSettings.TextColor = txtFgColor.Text;
        ResultSettings.FontSize = sliderFontSize.Value;
        ResultSettings.Bold = chkBold.IsChecked == true;
        ResultSettings.Opacity = sliderOpacity.Value;
        ResultSettings.CornerRadius = (int)sliderCorner.Value;

        DialogResult = true;
        Close();
    }
}
