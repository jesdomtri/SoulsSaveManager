namespace SoulsSaveManager.Services;

public sealed class BackupService : IBackupService
{
    public IReadOnlyList<string> ListBackups(string backupContextPath, IEnumerable<string>? legacyContextPaths = null)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddFrom(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;
            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                names.Add(Path.GetFileName(dir));
            }
        }

        AddFrom(backupContextPath);
        if (legacyContextPaths != null)
        {
            foreach (var legacy in legacyContextPaths)
            {
                AddFrom(legacy);
            }
        }

        return names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public void EnsureValidBackupName(string backupName)
    {
        if (string.IsNullOrWhiteSpace(backupName))
            throw new ArgumentException("El nombre del backup no puede estar vacío.", nameof(backupName));

        var trimmed = backupName.Trim();
        if (trimmed is "." or "..")
            throw new ArgumentException("El nombre del backup no es válido.", nameof(backupName));

        if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("El nombre del backup contiene caracteres no válidos.", nameof(backupName));

        if (Path.IsPathRooted(trimmed) || trimmed.Contains(Path.DirectorySeparatorChar) || trimmed.Contains(Path.AltDirectorySeparatorChar))
            throw new ArgumentException("El nombre del backup no es válido.", nameof(backupName));
    }

    public void CreateBackup(string sourceSavePath, string backupContextPath, string backupName)
    {
        EnsureValidBackupName(backupName);
        if (string.IsNullOrWhiteSpace(sourceSavePath) || !Directory.Exists(sourceSavePath))
            throw new DirectoryNotFoundException("La ubicación de guardado no existe o no es válida.");

        Directory.CreateDirectory(backupContextPath);
        var finalTarget = Path.Combine(backupContextPath, backupName.Trim());
        if (Directory.Exists(finalTarget))
            throw new IOException("Ya existe un backup con ese nombre.");

        var stagingParent = Path.Combine(Path.GetTempPath(), "SoulsSaveManager", "staging", Guid.NewGuid().ToString("N"));
        var stagingTarget = Path.Combine(stagingParent, "backup");
        Directory.CreateDirectory(stagingParent);
        try
        {
            CopyDirectoryRecursive(sourceSavePath, stagingTarget);
            try
            {
                Directory.Move(stagingTarget, finalTarget);
            }
            catch (IOException)
            {
                // Cross-volume or move issue: copy then clean staging
                CopyDirectoryRecursive(stagingTarget, finalTarget);
            }
        }
        catch
        {
            try { if (Directory.Exists(finalTarget)) Directory.Delete(finalTarget, true); } catch { }
            throw;
        }
        finally
        {
            try { if (Directory.Exists(stagingParent)) Directory.Delete(stagingParent, true); } catch { }
        }
    }

    public void RestoreBackup(string backupContextPath, string backupName, string targetSavePath, IEnumerable<string>? legacyContextPaths = null)
    {
        EnsureValidBackupName(backupName);
        if (string.IsNullOrWhiteSpace(targetSavePath))
            throw new ArgumentException("La ubicación de destino no es válida.", nameof(targetSavePath));

        var sourcePath = ResolveExistingBackupPath(backupContextPath, backupName, legacyContextPaths);
        if (sourcePath is null || !Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException("El backup seleccionado no existe.");

        Directory.CreateDirectory(targetSavePath);
        CopyDirectoryRecursive(sourcePath, targetSavePath, overwrite: true);
    }

    public void DeleteBackup(string backupContextPath, string backupName, IEnumerable<string>? legacyContextPaths = null)
    {
        EnsureValidBackupName(backupName);
        var sourcePath = ResolveExistingBackupPath(backupContextPath, backupName, legacyContextPaths);
        if (sourcePath is null || !Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException("El backup seleccionado no existe.");
        Directory.Delete(sourcePath, true);
    }

    public string? ResolveExistingBackupPath(string backupContextPath, string backupName, IEnumerable<string>? legacyContextPaths)
    {
        var name = backupName.Trim();
        var primary = Path.Combine(backupContextPath, name);
        if (Directory.Exists(primary)) return primary;
        if (legacyContextPaths != null)
        {
            foreach (var legacy in legacyContextPaths)
            {
                if (string.IsNullOrWhiteSpace(legacy)) continue;
                var candidate = Path.Combine(legacy, name);
                if (Directory.Exists(candidate)) return candidate;
            }
        }
        return null;
    }

    private static void CopyDirectoryRecursive(string sourceDir, string targetDir, bool overwrite = false)
    {
        Directory.CreateDirectory(targetDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var dest = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, dest, overwrite);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destDir = Path.Combine(targetDir, Path.GetFileName(dir));
            CopyDirectoryRecursive(dir, destDir, overwrite);
        }
    }
}
