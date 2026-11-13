namespace SoulsSaveManager.Services;

public interface ISettingsStore
{
    AppSettings Load();
    void Save(AppSettings settings);
    string SettingsPath { get; }
}
