namespace SoulsSaveManager.Services;

public interface IPathResolver
{
    string GetDefaultSaveRoot(string gameAlias);
    string GetSavePath(string gameAlias, string userId, string? customSaveLocation = null);
    string GetDefaultBackupRoot();
    string GetBackupPath(string gameAlias, string userId, string? backupRoot = null);
    IReadOnlyList<string> GetLegacyBackupPaths(string gameAlias, string userId);
}
