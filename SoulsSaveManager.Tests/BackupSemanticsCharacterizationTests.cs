namespace SoulsSaveManager.Tests;

public class BackupSemanticsCharacterizationTests
{
    [Fact]
    public void Current_create_copies_only_top_level_files()
    {
        using var temp = new TempDir();
        var source = temp.CreateDir("source");
        File.WriteAllText(Path.Combine(source, "a.txt"), "A");
        Directory.CreateDirectory(Path.Combine(source, "sub"));
        File.WriteAllText(Path.Combine(source, "sub", "b.txt"), "B");

        var target = temp.PathFor("backup");

        CurrentBehavior.CreateBackup(source, target);

        Assert.True(File.Exists(Path.Combine(target, "a.txt")));
        Assert.False(Directory.Exists(Path.Combine(target, "sub")));
        Assert.False(File.Exists(Path.Combine(target, "sub", "b.txt")));
    }

    [Fact]
    public void Current_load_copies_recursively_but_flattens_into_target()
    {
        using var temp = new TempDir();
        var backup = temp.CreateDir("backup");
        Directory.CreateDirectory(Path.Combine(backup, "nested"));
        File.WriteAllText(Path.Combine(backup, "root.txt"), "root");
        File.WriteAllText(Path.Combine(backup, "nested", "deep.txt"), "deep");

        var target = temp.CreateDir("save-target");

        CurrentBehavior.LoadBackup(backup, target);

        Assert.True(File.Exists(Path.Combine(target, "root.txt")));
        Assert.True(File.Exists(Path.Combine(target, "deep.txt")));
        Assert.False(Directory.Exists(Path.Combine(target, "nested")));
    }

    private static class CurrentBehavior
    {
        // Mirrors BackupsManager.NewBackup_Click copy logic
        public static void CreateBackup(string sourcePath, string targetPath)
        {
            Directory.CreateDirectory(targetPath);
            var files = Directory.GetFiles(sourcePath);
            foreach (var file in files)
            {
                var targetFile = Path.Combine(targetPath, Path.GetFileName(file));
                File.Copy(file, targetFile);
            }
        }

        // Mirrors BackupsManager.LoadBackup_Click copy logic
        public static void LoadBackup(string sourcePath, string targetPath)
        {
            Directory.CreateDirectory(targetPath);
            var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var targetFile = Path.Combine(targetPath, Path.GetFileName(file));
                File.Copy(file, targetFile, true);
            }
        }
    }

    private sealed class TempDir : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "SoulsSaveManagerTests", Guid.NewGuid().ToString("N"));

        public TempDir() => Directory.CreateDirectory(Root);

        public string CreateDir(string name)
        {
            var p = Path.Combine(Root, name);
            Directory.CreateDirectory(p);
            return p;
        }

        public string PathFor(string name) => Path.Combine(Root, name);

        public void Dispose()
        {
            try { if (Directory.Exists(Root)) Directory.Delete(Root, true); } catch { /* best effort */ }
        }
    }
}
