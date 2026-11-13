using System.Text.Json;

namespace SoulsSaveManager.Services;

public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly object _lock = new();

    public JsonSettingsStore(string? settingsPath = null, string? legacySearchDirectory = null)
    {
        var appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SoulsSaveManager");
        SettingsPath = settingsPath ?? Path.Combine(appDir, "appsettings.json");
        LegacySearchDirectory = legacySearchDirectory ?? AppContext.BaseDirectory;
    }

    public string SettingsPath { get; }
    public string LegacySearchDirectory { get; }

    public AppSettings Load()
    {
        lock (_lock)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);

            if (File.Exists(SettingsPath))
            {
                return Read(SettingsPath) ?? AppSettings.CreateDefault();
            }

            // Legacy migration: try common legacy names/locations (cwd/base dir)
            foreach (var legacy in GetLegacyCandidates())
            {
                if (File.Exists(legacy))
                {
                    var migrated = Read(legacy) ?? AppSettings.CreateDefault();
                    Normalize(migrated);
                    WriteAtomic(SettingsPath, migrated);
                    return migrated;
                }
            }

            var defaults = AppSettings.CreateDefault();
            Normalize(defaults);
            WriteAtomic(SettingsPath, defaults);
            return defaults;
        }
    }

    public void Save(AppSettings settings)
    {
        if (settings is null) throw new ArgumentNullException(nameof(settings));
        lock (_lock)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            Normalize(settings);
            WriteAtomic(SettingsPath, settings);
        }
    }

    private IEnumerable<string> GetLegacyCandidates()
    {
        var dirs = new[] { LegacySearchDirectory, Directory.GetCurrentDirectory() }
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        var names = new[] { "appsettings.json", "appSettings.json" };
        foreach (var dir in dirs)
        foreach (var name in names)
            yield return Path.Combine(dir, name);
    }

    private static AppSettings? Read(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return AppSettings.CreateDefault();
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? AppSettings.CreateDefault();
            Normalize(settings);
            return settings;
        }
        catch
        {
            return null;
        }
    }

    private static void Normalize(AppSettings settings)
    {
        settings.Users ??= new Dictionary<string, string>();
        settings.Locations ??= new Dictionary<string, string>();
        if (!settings.Locations.ContainsKey("DM"))
        {
            settings.Locations["DM"] = string.Empty;
        }
    }

    private static void WriteAtomic(string path, AppSettings settings)
    {
        var dir = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(dir);
        var temp = Path.Combine(dir, Path.GetRandomFileName());
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(temp, json);
        File.Move(temp, path, overwrite: true);
    }
}
