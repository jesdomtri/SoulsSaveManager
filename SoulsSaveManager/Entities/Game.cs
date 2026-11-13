namespace SoulsSaveManager.Entities
{
    public class Game
    {
        public Game(string alias)
        {
            Alias = alias;
            Name = GetGameName(alias);
            SaveDataPath = GetDefaultSaveRoot(alias, Name);
            BackupPath = GetDefaultGameBackupPath(Name);
        }

        public string Alias { get; set; }
        public string Name { get; set; }
        public string SaveDataPath { get; set; }
        public string BackupPath { get; set; }

        public static string GetGameName(string gameSelected) => gameSelected switch
        {
            "DS1" => "DARK SOULS REMASTERED",
            "DS2" => "DarkSoulsII",
            "DS3" => "DarkSoulsIII",
            "SKR" => "Sekiro",
            "ED" => "EldenRing",
            "DM" => "DemonSouls",
            _ => string.Empty
        };

        private static string GetDefaultSaveRoot(string gameSelected, string name)
        {
            return gameSelected switch
            {
                "DS1" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NBGI", name),
                "DS2" or "DS3" or "SKR" or "ED" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), name),
                "DM" => string.Empty,
                _ => string.Empty
            };
        }

        private static string GetDefaultGameBackupPath(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SoulsSaveManager", "Backups", name);
        }
    }
}
