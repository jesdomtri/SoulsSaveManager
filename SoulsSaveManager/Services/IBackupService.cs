namespace SoulsSaveManager.Services;

public interface IBackupService
{
    IReadOnlyList<string> ListBackups(string backupContextPath, IEnumerable<string>? legacyContextPaths = null);
    void CreateBackup(string sourceSavePath, string backupContextPath, string backupName);
    void RestoreBackup(string backupContextPath, string backupName, string targetSavePath, IEnumerable<string>? legacyContextPaths = null);
    void DeleteBackup(string backupContextPath, string backupName, IEnumerable<string>? legacyContextPaths = null);
    void EnsureValidBackupName(string backupName);
    string? ResolveExistingBackupPath(string backupContextPath, string backupName, IEnumerable<string>? legacyContextPaths);
}
