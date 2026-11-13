using System.Net;
using System.Net.Http;
using SoulsSaveManager.Services;

namespace SoulsSaveManager.Tests;

public class UserProfileResolverTests
{
    [Fact]
    public void DiscoverProfiles_returns_empty_for_missing_or_dm()
    {
        using var temp = new TempDir();
        var store = new JsonSettingsStore(Path.Combine(temp.Root, "appsettings.json"), temp.Root);
        var resolver = new UserProfileResolver(store);

        Assert.Empty(resolver.DiscoverProfiles("DM", ""));
        Assert.Empty(resolver.DiscoverProfiles("DS3", Path.Combine(temp.Root, "nope")));
    }

    [Fact]
    public void DiscoverProfiles_lists_profile_directories()
    {
        using var temp = new TempDir();
        var saveRoot = temp.Dir("saves");
        temp.Dir(Path.Combine("saves", "111"));
        temp.Dir(Path.Combine("saves", "222"));
        File.WriteAllText(Path.Combine(saveRoot, "file.txt"), "x");
        var store = new JsonSettingsStore(Path.Combine(temp.Root, "appsettings.json"), temp.Root);
        var resolver = new UserProfileResolver(store);

        var profiles = resolver.DiscoverProfiles("DS3", saveRoot);

        Assert.Equal(new[] { "111", "222" }, profiles);
    }

    [Fact]
    public void GetDisplayLabel_uses_cached_name()
    {
        using var temp = new TempDir();
        var store = new JsonSettingsStore(Path.Combine(temp.Root, "appsettings.json"), temp.Root);
        var settings = store.Load();
        settings.Users["123"] = "Player";
        store.Save(settings);
        var resolver = new UserProfileResolver(store);

        Assert.Equal("123 - Player", resolver.GetDisplayLabel("123", "DS3"));
    }

    [Fact]
    public async Task ResolveLabelAsync_falls_back_when_external_resolution_fails()
    {
        using var temp = new TempDir();
        var store = new JsonSettingsStore(Path.Combine(temp.Root, "appsettings.json"), temp.Root);
        var http = new HttpClient(new ThrowHandler()) { Timeout = TimeSpan.FromMilliseconds(200) };
        var resolver = new UserProfileResolver(store, http);

        var label = await resolver.ResolveLabelAsync("999", "DS3");

        Assert.Equal("999", label);
    }

    [Fact]
    public void GetSteamId_formats_ds1_numeric_ids()
    {
        Assert.Equal("[U:1:123]", UserProfileResolver.GetSteamId("123", "DS1"));
        Assert.Equal("123", UserProfileResolver.GetSteamId("123", "DS3"));
    }

    private sealed class ThrowHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("boom");
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
        public void Dispose() { try { if (Directory.Exists(Root)) Directory.Delete(Root, true); } catch { } }
    }
}
