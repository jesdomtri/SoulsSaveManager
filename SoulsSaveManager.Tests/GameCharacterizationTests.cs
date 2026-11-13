using SoulsSaveManager.Entities;

namespace SoulsSaveManager.Tests;

public class GameCharacterizationTests
{
    [Theory]
    [InlineData("DS1", "DARK SOULS REMASTERED")]
    [InlineData("DS2", "DarkSoulsII")]
    [InlineData("DS3", "DarkSoulsIII")]
    [InlineData("SKR", "Sekiro")]
    [InlineData("ED", "EldenRing")]
    [InlineData("DM", "DemonSouls")]
    public void Constructor_sets_expected_name(string alias, string expectedName)
    {
        var game = new Game(alias);

        Assert.Equal(expectedName, game.Name);
    }

    [Theory]
    [InlineData("DS2")]
    [InlineData("DS3")]
    [InlineData("SKR")]
    [InlineData("ED")]
    public void Constructor_sets_absolute_roaming_save_root_for_non_ds1_non_dm(string alias)
    {
        var game = new Game(alias);

        var expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), game.Name);
        Assert.Equal(expected, game.SaveDataPath);
        Assert.True(Path.IsPathRooted(game.SaveDataPath));
    }

    [Fact]
    public void Constructor_sets_absolute_documents_nbgi_path_for_ds1()
    {
        var game = new Game("DS1");

        var expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NBGI", game.Name);
        Assert.Equal(expected, game.SaveDataPath);
    }

    [Fact]
    public void Constructor_sets_empty_save_path_for_dm()
    {
        var game = new Game("DM");

        Assert.Equal(string.Empty, game.SaveDataPath);
    }

    [Theory]
    [InlineData("DS1", "DARK SOULS REMASTERED")]
    [InlineData("DM", "DemonSouls")]
    [InlineData("ED", "EldenRing")]
    public void Constructor_sets_default_absolute_backup_path_under_localappdata(string alias, string name)
    {
        var game = new Game(alias);

        var expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SoulsSaveManager", "Backups", name);
        Assert.Equal(expected, game.BackupPath);
        Assert.True(Path.IsPathRooted(game.BackupPath));
    }

    [Fact]
    public void Constructor_unknown_alias_produces_empty_name_and_paths()
    {
        var game = new Game("UNKNOWN");

        Assert.Equal(string.Empty, game.Name);
        Assert.Equal(string.Empty, game.SaveDataPath);
        Assert.Equal(string.Empty, game.BackupPath);
    }
}
