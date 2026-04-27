using System;
using System.IO;
using System.Text.Json;

namespace MachineLabel;

public class LabelSettings
{
    public string LabelText { get; set; } = "";
    public string BackgroundColor { get; set; } = "#FF6B35";
    public string TextColor { get; set; } = "#FFFFFF";
    public double FontSize { get; set; } = 13;
    public bool Bold { get; set; } = true;
    public int OffsetX { get; set; } = 0;
    public bool StartWithWindows { get; set; } = false;
    public double Opacity { get; set; } = 0.95;
    public int CornerRadius { get; set; } = 4;
    public int PaddingH { get; set; } = 10;
    public int PaddingV { get; set; } = 2;

    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MachineLabel");
    private static readonly string SettingsFile = Path.Combine(SettingsDir, "settings.json");

    public static LabelSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<LabelSettings>(json) ?? CreateDefault();
            }
        }
        catch { }
        return CreateDefault();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        }
        catch { }
    }

    private static LabelSettings CreateDefault()
    {
        return new LabelSettings
        {
            LabelText = $"🖥️ {Environment.MachineName}"
        };
    }

    public void SetStartWithWindows(bool enable)
    {
        StartWithWindows = enable;
        try
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;

            if (enable)
            {
                var exePath = Environment.ProcessPath ?? "";
                if (!string.IsNullOrEmpty(exePath))
                    key.SetValue("MachineLabel", $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue("MachineLabel", false);
            }
            key.Close();
        }
        catch { }
    }
}
