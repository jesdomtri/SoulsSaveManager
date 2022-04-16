namespace SoulsSaveManager.Entities
{
    public class Game
    {
        public Game(string alias)
        {
            Alias = alias;
            Name = SelectTitleGame(alias);
            SaveDataPath = SelectLocationGame(alias, Name);
            BackupPath = $".\\Backups\\{Name}";
        }

        public string Alias { get; set; }
        public string Name { get; set; }
        public string SaveDataPath { get; set; }
        public string BackupPath { get; set; }

        private static string SelectTitleGame(string gameSelected)
        {
            switch (gameSelected)
            {
                case "DS1":
                    return "DARK SOULS REMASTERED";
                case "DS2":
                    return "DarkSoulsII";
                case "DS3":
                    return "DarkSoulsIII";
                case "SKR":
                    return "Sekiro";
                case "ED":
                    return "EldenRing";
                case "DM":
                    return "DemonSouls";
                default:
                    return "";
            }
        }

        private static string SelectLocationGame(string gameSelected, string name)
        {
            switch (gameSelected)
            {
                case "DS1":
                    return $"\\Users\\{Environment.UserName}\\Documents\\NBGI\\{name}";
                case "DM":
                    return "";
                default:
                    return $"\\Users\\{Environment.UserName}\\AppData\\Roaming\\{name}";
            }
        }
    }
}
