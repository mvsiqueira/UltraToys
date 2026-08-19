using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using UltraToys.Models;

namespace UltraToys.Services;

/// <summary>Loads/saves settings.json next to the executable. Never throws — corrupt or missing
/// files fall back to defaults so a bad settings file can never prevent the app from starting.</summary>
public static class PersistenceService
{
    public static string SettingsPath => Path.Combine(AppContext.BaseDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    public static AppSettings LoadSettings()
    {
        if (!File.Exists(SettingsPath))
            return new AppSettings();

        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void SaveSettings(AppSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Best-effort persistence — a save failure (e.g. read-only folder) should not crash the app.
        }
    }
}
