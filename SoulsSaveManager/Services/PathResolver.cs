namespace SoulsSaveManager.Services;

public sealed class PathResolver : IPathResolver
{
    private readonly string _appDataRoot;

    public PathResolver(string? appDataRoot = null)
    {
        _appDataRoot = appDataRoot ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SoulsSaveManager");
    }

    public string GetDefaultSaveRoot(string gameAlias)
    {
        return gameAlias switch
        {
            "DS1" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NBGI", "DARK SOULS REMASTERED"),
            "DS2" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DarkSoulsII"),
            "DS3" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DarkSoulsIII"),
            "SKR" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sekiro"),
            "ED" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EldenRing"),
            "DM" => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(gameAlias), gameAlias, "Unsupported game alias")
        };
    }

    public string GetSavePath(string gameAlias, string userId, string? customSaveLocation = null)
    {
        if (!string.IsNullOrWhiteSpace(customSaveLocation))
        {
            return Path.GetFullPath(customSaveLocation);
        }

        if (gameAlias == "DM")
        {
            return string.Empty;
        }

        var root = GetDefaultSaveRoot(gameAlias);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return root;
        }

        return Path.Combine(root, userId);
    }

    public string GetDefaultBackupRoot()
    {
        return Path.Combine(_appDataRoot, "Backups");
    }

    public string GetBackupPath(string gameAlias, string userId, string? backupRoot = null)
    {
        var gameName = GetGameName(gameAlias);
        var root = string.IsNullOrWhiteSpace(backupRoot) ? GetDefaultBackupRoot() : Path.GetFullPath(backupRoot);
        var path = Path.Combine(root, gameName);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            path = Path.Combine(path, userId);
        }
        return path;
    }

    public IReadOnlyList<string> GetLegacyBackupPaths(string gameAlias, string userId)
    {
        var gameName = GetGameName(gameAlias);
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Backups", gameName, userId ?? string.Empty),
            Path.Combine(Directory.GetCurrentDirectory(), "Backups", gameName, userId ?? string.Empty),
        };
        return candidates
            .Select(p => Path.GetFullPath(p.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string GetGameName(string gameAlias) => gameAlias switch
    {
        "DS1" => "DARK SOULS REMASTERED",
        "DS2" => "DarkSoulsII",
        "DS3" => "DarkSoulsIII",
        "SKR" => "Sekiro",
        "ED" => "EldenRing",
        "DM" => "DemonSouls",
        _ => throw new ArgumentOutOfRangeException(nameof(gameAlias), gameAlias, "Unsupported game alias")
    };
}
