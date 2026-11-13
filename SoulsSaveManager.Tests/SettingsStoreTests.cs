using SoulsSaveManager.Services;

namespace SoulsSaveManager.Tests;

public class SettingsStoreTests
{
    [Fact]
    public void Load_creates_default_settings_when_missing()
    {
        using var temp = new TempDir();
        var path = Path.Combine(temp.Root, "cfg", "appsettings.json");
        var store = new JsonSettingsStore(path, temp.Root);

        var settings = store.Load();

        Assert.True(File.Exists(path));
        Assert.NotNull(settings.Users);
        Assert.NotNull(settings.Locations);
        Assert.True(settings.Locations.ContainsKey("DM"));
    }

    [Fact]
    public void Save_and_Load_persist_values()
    {
        using var temp = new TempDir();
        var path = Path.Combine(temp.Root, "appsettings.json");
        var store = new JsonSettingsStore(path, temp.Root);

        var settings = store.Load();
        settings.BackupRoot = Path.Combine(temp.Root, "backups");
        settings.Users["123"] = "Tester";
        settings.Locations["DM"] = "X:\\saves";
        store.Save(settings);

        var reloaded = new JsonSettingsStore(path, temp.Root).Load();

        Assert.Equal(settings.BackupRoot, reloaded.BackupRoot);
        Assert.Equal("Tester", reloaded.Users["123"]);
        Assert.Equal("X:\\saves", reloaded.Locations["DM"]);
    }

    [Fact]
    public void Load_migrates_from_legacy_appSettings_casing_in_search_directory()
    {
        using var temp = new TempDir();
        var canonical = Path.Combine(temp.Root, "state", "appsettings.json");
        var legacy = Path.Combine(temp.Root, "appSettings.json");
        File.WriteAllText(legacy, "{\n  \"users\": { \"u1\": \"Legacy\" },\n  \"locations\": { \"DM\": \"C:/dm\" }\n}");

        var store = new JsonSettingsStore(canonical, temp.Root);
        var settings = store.Load();

        Assert.Equal("Legacy", settings.Users["u1"]);
        Assert.Equal("C:/dm", settings.Locations["DM"]);
        Assert.True(File.Exists(canonical));
    }

    [Fact]
    public void Save_overwrites_atomically_and_can_be_reloaded()
    {
        using var temp = new TempDir();
        var path = Path.Combine(temp.Root, "appsettings.json");
        var store = new JsonSettingsStore(path, temp.Root);

        var a = store.Load();
        a.Users["k"] = "v1";
        store.Save(a);

        var b = store.Load();
        b.Users["k"] = "v2";
        store.Save(b);

        var reloaded = store.Load();
        Assert.Equal("v2", reloaded.Users["k"]);
    }

    private sealed class TempDir : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "SoulsSaveManagerTests", Guid.NewGuid().ToString("N"));
        public TempDir() => Directory.CreateDirectory(Root);
        public void Dispose()
        {
            try { if (Directory.Exists(Root)) Directory.Delete(Root, true); } catch { }
        }
    }
}
