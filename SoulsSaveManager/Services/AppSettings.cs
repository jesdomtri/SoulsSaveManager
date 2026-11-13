using System.Text.Json.Serialization;

namespace SoulsSaveManager.Services;

public sealed class AppSettings
{
    [JsonPropertyName("backupRoot")]
    public string? BackupRoot { get; set; }

    [JsonPropertyName("users")]
    public Dictionary<string, string> Users { get; set; } = new();

    [JsonPropertyName("locations")]
    public Dictionary<string, string> Locations { get; set; } = new();

    public static AppSettings CreateDefault()
    {
        return new AppSettings
        {
            BackupRoot = null,
            Users = new Dictionary<string, string>(),
            Locations = new Dictionary<string, string>
            {
                ["DM"] = string.Empty
            }
        };
    }
}
