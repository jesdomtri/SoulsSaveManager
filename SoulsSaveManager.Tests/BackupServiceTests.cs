using SoulsSaveManager.Services;

namespace SoulsSaveManager.Tests;

public class BackupServiceTests
{
    [Fact]
    public void CreateBackup_copies_recursively_and_preserves_structure()
    {
        using var temp = new TempDir();
        var source = temp.Dir("source");
        File.WriteAllText(Path.Combine(source, "root.txt"), "root");
        Directory.CreateDirectory(Path.Combine(source, "sub", "deep"));
        File.WriteAllText(Path.Combine(source, "sub", "a.txt"), "A");
        File.WriteAllText(Path.Combine(source, "sub", "deep", "b.txt"), "B");

        var backupRoot = temp.Dir("backups");
        var svc = new BackupService();

        svc.CreateBackup(source, backupRoot, "slot1");

        var created = Path.Combine(backupRoot, "slot1");
        Assert.True(File.Exists(Path.Combine(created, "root.txt")));
        Assert.Equal("A", File.ReadAllText(Path.Combine(created, "sub", "a.txt")));
        Assert.Equal("B", File.ReadAllText(Path.Combine(created, "sub", "deep", "b.txt")));
    }

    [Fact]
    public void RestoreBackup_copies_recursively_and_preserves_structure()
    {
        using var temp = new TempDir();
        var backupRoot = temp.Dir("backups");
        var slot = temp.Dir(Path.Combine("backups", "slot1"));
        Directory.CreateDirectory(Path.Combine(slot, "nested"));
        File.WriteAllText(Path.Combine(slot, "nested", "x.txt"), "X");
        File.WriteAllText(Path.Combine(slot, "y.txt"), "Y");

        var target = temp.Dir("target");
        var svc = new BackupService();

        svc.RestoreBackup(backupRoot, "slot1", target);

        Assert.Equal("X", File.ReadAllText(Path.Combine(target, "nested", "x.txt")));
        Assert.Equal("Y", File.ReadAllText(Path.Combine(target, "y.txt")));
    }

    [Fact]
    public void RestoreBackup_overwrites_existing_files()
    {
        using var temp = new TempDir();
        var backupRoot = temp.Dir("backups");
        var slot = temp.Dir(Path.Combine("backups", "slot1"));
        File.WriteAllText(Path.Combine(slot, "a.txt"), "new");
        var target = temp.Dir("target");
        File.WriteAllText(Path.Combine(target, "a.txt"), "old");

        new BackupService().RestoreBackup(backupRoot, "slot1", target);

        Assert.Equal("new", File.ReadAllText(Path.Combine(target, "a.txt")));
    }

    [Fact]
    public void CreateBackup_rejects_invalid_or_duplicate_names()
    {
        using var temp = new TempDir();
        var source = temp.Dir("source");
        File.WriteAllText(Path.Combine(source, "a.txt"), "A");
        var backupRoot = temp.Dir("backups");
        var svc = new BackupService();
        svc.CreateBackup(source, backupRoot, "slot1");

        Assert.Throws<ArgumentException>(() => svc.CreateBackup(source, backupRoot, ""));
        Assert.Throws<ArgumentException>(() => svc.CreateBackup(source, backupRoot, "../evil"));
        Assert.Throws<IOException>(() => svc.CreateBackup(source, backupRoot, "slot1"));
    }

    [Fact]
    public void CreateBackup_throws_when_source_missing()
    {
        using var temp = new TempDir();
        var svc = new BackupService();

        Assert.Throws<DirectoryNotFoundException>(() => svc.CreateBackup(Path.Combine(temp.Root, "missing"), temp.Dir("b"), "s1"));
    }

    [Fact]
    public void RestoreBackup_throws_when_backup_missing()
    {
        using var temp = new TempDir();
        var svc = new BackupService();

        Assert.Throws<DirectoryNotFoundException>(() => svc.RestoreBackup(temp.Dir("b"), "nope", temp.Dir("t")));
    }

    [Fact]
    public void DeleteBackup_removes_selected_backup_only()
    {
        using var temp = new TempDir();
        var backupRoot = temp.Dir("backups");
        temp.Dir(Path.Combine("backups", "a"));
        temp.Dir(Path.Combine("backups", "b"));
        var svc = new BackupService();

        svc.DeleteBackup(backupRoot, "a");

        Assert.False(Directory.Exists(Path.Combine(backupRoot, "a")));
        Assert.True(Directory.Exists(Path.Combine(backupRoot, "b")));
    }

    [Fact]
    public void ListBackups_discovers_primary_and_legacy_locations()
    {
        using var temp = new TempDir();
        var primary = temp.Dir("primary");
        var legacy = temp.Dir("legacy");
        temp.Dir(Path.Combine("primary", "newslot"));
        temp.Dir(Path.Combine("legacy", "oldslot"));
        temp.Dir(Path.Combine("legacy", "newslot")); // duplicate name
        var svc = new BackupService();

        var list = svc.ListBackups(primary, new[] { legacy });

        Assert.Equal(new[] { "newslot", "oldslot" }, list);
    }

    [Fact]
    public void Restore_and_Delete_can_operate_on_legacy_discovered_backup()
    {
        using var temp = new TempDir();
        var primary = temp.Dir("primary");
        var legacy = temp.Dir("legacy");
        var slot = temp.Dir(Path.Combine("legacy", "oldslot"));
        File.WriteAllText(Path.Combine(slot, "f.txt"), "legacy");
        var target = temp.Dir("target");
        var svc = new BackupService();

        svc.RestoreBackup(primary, "oldslot", target, new[] { legacy });
        Assert.Equal("legacy", File.ReadAllText(Path.Combine(target, "f.txt")));

        svc.DeleteBackup(primary, "oldslot", new[] { legacy });
        Assert.False(Directory.Exists(slot));
    }

    private sealed class TempDir : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "SoulsSaveManagerTests", Guid.NewGuid().ToString("N"));
        public TempDir() => Directory.CreateDirectory(Root);
        public string Dir(string relative)
        {
            var p = Path.Combine(Root, relative);
            Directory.CreateDirectory(p);
            return p;
        }
        public void Dispose()
        {
            try { if (Directory.Exists(Root)) Directory.Delete(Root, true); } catch { }
        }
    }
}
