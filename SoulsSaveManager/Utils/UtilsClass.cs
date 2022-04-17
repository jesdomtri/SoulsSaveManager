namespace SoulsSaveManager.Utils
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
            string result = string.Empty;
            bool exist = App.UsersCache != null ? App.UsersCache.ContainsKey(userID) : false;

            if (exist)
            {
                result = App.UsersCache != null ? $"{userID} - {App.UsersCache[userID]}" : userID;
            }
            else
            {
                HtmlWeb oWeb = new();
                HtmlAgilityPack.HtmlDocument doc = oWeb.Load($"https://steamcommunity.com/profiles/{GetUserIDWeb(userID)}/");
                string username = doc.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("class", "").Contains("actual_persona_name")).ToList()[0].InnerHtml;
                result = !string.IsNullOrEmpty(username) ? $"{userID} - {username}" : userID;
                OverwriteUsersAppSettings($"users:{userID}", username);
            }
            return result;
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

        public void OverwriteUsersAppSettings(string key, string value)
        {
            try
            {
                string? appsettingsPath = Path.Combine(AppContext.BaseDirectory, "appSettings.json");
                string json = File.ReadAllText(appsettingsPath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                string? sectionPath = key.Split(":")[0];
                string? keyPath = key.Split(":")[1];
                jsonObj[sectionPath][keyPath] = value;
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(appsettingsPath, output);
                App.UsersCache = App.Configuration?.GetSection("users").Get<Dictionary<string, string>>();
                App.LocationsCache = App.Configuration?.GetSection("locations").Get<Dictionary<string, string>>();
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Something unexpected happened", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
