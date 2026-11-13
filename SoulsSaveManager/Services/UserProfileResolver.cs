using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SoulsSaveManager.Services;

public sealed class UserProfileResolver : IUserProfileResolver
{
    private readonly ISettingsStore _settingsStore;
    private readonly HttpClient _httpClient;

    public UserProfileResolver(ISettingsStore settingsStore, HttpClient? httpClient = null)
    {
        _settingsStore = settingsStore;
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    }

    public IReadOnlyList<string> DiscoverProfiles(string gameAlias, string saveRootOrCustomPath)
    {
        if (gameAlias == "DM")
        {
            // DemonSouls in RPCS3 uses custom save location selection; no standard user subdirs.
            return Array.Empty<string>();
        }

        if (string.IsNullOrWhiteSpace(saveRootOrCustomPath) || !Directory.Exists(saveRootOrCustomPath))
        {
            return Array.Empty<string>();
        }

        try
        {
            return Directory.EnumerateDirectories(saveRootOrCustomPath)
                .Select(Path.GetFileName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Cast<string>()
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public string GetDisplayLabel(string profileId, string gameAlias)
    {
        if (gameAlias == "DM") return "NOT USER FOR DEMON SOULS IN RPCS3";
        var settings = _settingsStore.Load();
        if (settings.Users.TryGetValue(profileId, out var cached) && !string.IsNullOrWhiteSpace(cached))
        {
            return $"{profileId} - {cached}";
        }
        return profileId;
    }

    public async Task<string> ResolveLabelAsync(string profileId, string gameAlias, CancellationToken cancellationToken = default)
    {
        if (gameAlias == "DM") return "NOT USER FOR DEMON SOULS IN RPCS3";

        var settings = _settingsStore.Load();
        if (settings.Users.TryGetValue(profileId, out var cached) && !string.IsNullOrWhiteSpace(cached))
        {
            return $"{profileId} - {cached}";
        }

        try
        {
            var steamId = GetSteamId(profileId, gameAlias);
            var url = $"https://steamcommunity.com/profiles/{steamId}/?xml=1";
            using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var xml = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var name = ExtractSteamPersonaName(xml) ?? profileId;
            settings.Users[profileId] = name;
            _settingsStore.Save(settings);
            return $"{profileId} - {name}";
        }
        catch
        {
            return profileId;
        }
    }

    public static string GetSteamId(string profileId, string gameAlias)
    {
        if (long.TryParse(profileId, out _))
        {
            return gameAlias == "DS1" ? $"[U:1:{profileId}]" : profileId;
        }
        if (long.TryParse(profileId, System.Globalization.NumberStyles.HexNumber, null, out var asDec))
        {
            return asDec.ToString();
        }
        return profileId;
    }

    private static string? ExtractSteamPersonaName(string xml)
    {
        // XML endpoint returns <steamID><![CDATA[name]]></steamID>
        const string open = "<steamID>";
        const string close = "</steamID>";
        var i = xml.IndexOf(open, StringComparison.OrdinalIgnoreCase);
        if (i < 0) return null;
        i += open.Length;
        var j = xml.IndexOf(close, i, StringComparison.OrdinalIgnoreCase);
        if (j < 0) return null;
        var value = xml.Substring(i, j - i).Trim();
        value = value.Replace("<![CDATA[", string.Empty).Replace("]]>", string.Empty).Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
