namespace SoulsSaveManager.Entities
{
    public class Game
    {
        public Game(string alias)
        {
            Alias = alias;
            Name = SelectTitleGame(alias);
            SaveDataPath = (Alias.Equals("DS1") ?
                $"\\Users\\{Environment.UserName}\\Documents\\NBGI\\{Name}" :
                $"\\Users\\{Environment.UserName}\\AppData\\Roaming\\{Name}");
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
                default:
                    return "";
            }
        }
    }
}
