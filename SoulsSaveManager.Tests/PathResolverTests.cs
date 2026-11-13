using SoulsSaveManager.Services;

namespace SoulsSaveManager.Tests;

public class PathResolverTests
{
    [Theory]
    [InlineData("DS2", "DarkSoulsII")]
    [InlineData("DS3", "DarkSoulsIII")]
    [InlineData("SKR", "Sekiro")]
    [InlineData("ED", "EldenRing")]
    public void GetDefaultSaveRoot_returns_absolute_roaming_path(string alias, string expectedName)
    {
        var resolver = new PathResolver();

        var root = resolver.GetDefaultSaveRoot(alias);

        var expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), expectedName);
        Assert.Equal(expected, root);
        Assert.True(Path.IsPathRooted(root));
    }

    [Fact]
    public void GetDefaultSaveRoot_ds1_uses_documents_nbgi()
    {
        var resolver = new PathResolver();

        var root = resolver.GetDefaultSaveRoot("DS1");

        var expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NBGI", "DARK SOULS REMASTERED");
        Assert.Equal(expected, root);
    }

    [Fact]
    public void GetDefaultSaveRoot_dm_is_empty()
    {
        var resolver = new PathResolver();

        Assert.Equal(string.Empty, resolver.GetDefaultSaveRoot("DM"));
    }

    [Theory]
    [InlineData("DS1", "user1")]
    [InlineData("DS2", "user2")]
    [InlineData("DS3", "user3")]
    [InlineData("SKR", "user4")]
    [InlineData("ED", "user5")]
    public void GetSavePath_returns_absolute_path_with_user_context(string alias, string user)
    {
        var resolver = new PathResolver();

        var path = resolver.GetSavePath(alias, user);

        Assert.True(Path.IsPathRooted(path));
        Assert.EndsWith(Path.DirectorySeparatorChar + user, path.Replace('/', Path.DirectorySeparatorChar));
    }

    [Fact]
    public void GetSavePath_dm_without_custom_location_is_empty()
    {
        var resolver = new PathResolver();

        Assert.Equal(string.Empty, resolver.GetSavePath("DM", "any"));
    }

    [Fact]
    public void GetSavePath_uses_custom_location_when_provided()
    {
        var resolver = new PathResolver();
        var custom = Path.Combine(Path.GetTempPath(), "custom-save");

        var path = resolver.GetSavePath("ED", "user", custom);

        Assert.Equal(Path.GetFullPath(custom), path);
    }

    [Fact]
    public void GetBackupPath_is_absolute_and_independent_of_working_directory_shape()
    {
        var resolver = new PathResolver(Path.Combine(Path.GetTempPath(), "ssm-root"));

        var path = resolver.GetBackupPath("DS3", "u1");

        Assert.True(Path.IsPathRooted(path));
        Assert.Contains("DarkSoulsIII", path);
        Assert.EndsWith(Path.Combine("DarkSoulsIII", "u1"), path);
    }

    [Fact]
    public void GetBackupPath_respects_custom_backup_root()
    {
        var resolver = new PathResolver();
        var root = Path.Combine(Path.GetTempPath(), "my-backups");

        var path = resolver.GetBackupPath("DM", "profile", root);

        Assert.Equal(Path.Combine(Path.GetFullPath(root), "DemonSouls", "profile"), path);
    }
}
