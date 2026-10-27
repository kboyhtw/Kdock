using System;
using System.IO;
using System.Text.Json;
using System.Windows.Media;

namespace Kdock.App;

public sealed class AppSettings
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly string _path;

    public AppSettings(string dataDirectory)
    {
        _path = Path.Combine(dataDirectory, "settings.json");
        Load();
    }

    public static AppSettings Current { get; set; } = null!;

    public string Theme { get; set; } = "Dark";
    public bool ClipboardSharing { get; set; } = true;
    public event Action? Changed;

    public void Save()
    {
        try
        {
            File.WriteAllText(_path, JsonSerializer.Serialize(new Stored { Theme = Theme, ClipboardSharing = ClipboardSharing }, Json));
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        Changed?.Invoke();
    }

    public void ApplyTheme()
    {
        if (System.Windows.Application.Current == null)
            return;
        bool light = Theme.Equals("Light", StringComparison.OrdinalIgnoreCase);
        Set("SurfaceBrush", light ? "#F3F3F3" : "#1F1F1F");
        Set("SurfaceRaisedBrush", light ? "#FFFFFF" : "#252526");
        Set("SurfaceInsetBrush", light ? "#EBEBEB" : "#181818");
        Set("SurfaceCardBrush", light ? "#FFFFFF" : "#2C2C2C");
        Set("AccentBrush", light ? "#0078D4" : "#60CDFF");
        Set("TextBrush", light ? "#1A1A1A" : "#FFFFFF");
        Set("MutedTextBrush", light ? "#3B3B3B" : "#C8C8C8");
        Set("SubtleTextBrush", light ? "#4A4A4A" : "#ADADAD");
        Set("BorderBrush", light ? "#D1D1D1" : "#3F3F3F");
        Set("MineBrush", light ? "#D6EBFA" : "#094771");
        Set("TheirsBrush", light ? "#FFFFFF" : "#3A3A3A");
        Set("HoverBrush", light ? "#C9DCEA" : "#3E4F5E");
        Set("PressBrush", light ? "#AFCDE3" : "#324554");
        Set("SelectedBrush", light ? "#A8D4F0" : "#0C3B5E");
        Set("ScrollThumbBrush", light ? "#8A000000" : "#5AFFFFFF");
        WindowSkin.RefreshAll();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_path))
                return;
            Stored? stored = JsonSerializer.Deserialize<Stored>(File.ReadAllText(_path), Json);
            if (stored == null)
                return;
            if (!string.IsNullOrWhiteSpace(stored.Theme))
                Theme = stored.Theme;
            ClipboardSharing = stored.ClipboardSharing;
        }
        catch (IOException)
        {
        }
        catch (JsonException)
        {
        }
    }

    private static void Set(string key, string hex)
    {
        var resources = System.Windows.Application.Current?.Resources;
        if (resources == null)
            return;
        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
        resources[key] = new SolidColorBrush(color);
    }

    private sealed class Stored
    {
        public string Theme { get; set; } = "Dark";
        public bool ClipboardSharing { get; set; } = true;
    }
}
