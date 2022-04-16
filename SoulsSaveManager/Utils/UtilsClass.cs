﻿namespace SoulsSaveManager.Utils
{
    public class UtilsClass
    {
        private Game _game;
        public UtilsClass(Game game)
        {
            _game = game;
        }

        public List<string>? LoadUsersComboBox()
        {
            List<string>? listUsers = new List<string>();
            if (_game.Alias.Equals("DM"))
            {
                listUsers.Add("NOT USER FOR DEMON SOULS IN RPCS3");
            }
            else
            {
                if (Directory.Exists(_game.SaveDataPath))
                {
                    foreach (var user in Directory.EnumerateDirectories(_game.SaveDataPath))
                    {
                        string userID = user.Split("\\").Last();
                        listUsers.Add(GetCompleteUser(userID));
                    }
                }
            }
            return listUsers;
        }

        public void LoadBackupsComboBox(System.Windows.Controls.ComboBox backupsComboBox, string userBackupPath)
        {
            ObservableCollection<string>? listBackups = new ObservableCollection<string>();
            try
            {
                if (Directory.Exists(userBackupPath))
                {
                    foreach (string? backup in Directory.EnumerateDirectories(userBackupPath))
                    {
                        listBackups.Add(backup.Split("\\").Last());
                    }
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("Something unexpected happened", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            backupsComboBox.ItemsSource = listBackups;
            backupsComboBox.SelectedIndex = 0;
        }

        public string GetCompleteUser(string userID)
        {
            HtmlWeb oWeb = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = oWeb.Load($"https://steamcommunity.com/profiles/{GetUserIDWeb(userID)}/");
            string username = doc.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("class", "").Contains("actual_persona_name")).ToList()[0].InnerHtml;
            return !string.IsNullOrEmpty(username) ? $"{userID} - {username}" : userID;
        }

        public string GetUserIDWeb(string userIDOriginal)
        {
            string userIDWeb = userIDOriginal;
            if (!long.TryParse(userIDOriginal, out _))
                userIDWeb = long.Parse(userIDOriginal, System.Globalization.NumberStyles.HexNumber).ToString();
            if (_game.Alias.Equals("DS1"))
                userIDWeb = $"[U:1:{userIDOriginal}]";
            return userIDWeb;
        }
    }
}
