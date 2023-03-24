namespace SoulsSaveManager.Utils
{
    public class UtilsClass
    {
        private Game _game;
        public UtilsClass(Game game)
        {
            _game = game;
        }

        public List<string> LoadUsersComboBox()
        {
            if (_game.Alias.Equals("DM"))
            {
                return new List<string> { "NOT USER FOR DEMON SOULS IN RPCS3" };
            }

            try
            {
                var directories = Directory.EnumerateDirectories(_game.SaveDataPath);
                return directories.Select(user => GetCompleteUser(user.Split("\\").Last())).ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Something unexpected happened: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<string>();
            }
        }


        public static void LoadBackupsComboBox(System.Windows.Controls.ComboBox backupsComboBox, string userBackupPath)
        {
            try
            {
                if (Directory.Exists(userBackupPath))
                {
                    var listBackups = Directory.EnumerateDirectories(userBackupPath).Select(backup => backup.Split("\\").Last());
                    backupsComboBox.ItemsSource = new ObservableCollection<string>(listBackups);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Something unexpected happened: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (backupsComboBox.HasItems)
            {
                backupsComboBox.SelectedIndex = 0;
            }
        }

        public string GetCompleteUser(string userID)
        {
            if (App.UsersCache != null && App.UsersCache.TryGetValue(userID, out string cachedUsername))
            {
                return $"{userID} - {cachedUsername}";
            }

            HtmlWeb oWeb = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = oWeb.Load($"https://steamcommunity.com/profiles/{GetUserIDWeb(userID)}/");
            string? username = doc.DocumentNode.Descendants("span").FirstOrDefault(node => node.GetAttributeValue("class", "").Contains("actual_persona_name"))?.InnerHtml;

            if (string.IsNullOrEmpty(username))
            {
                username = userID;
            }

            OverwriteUsersAppSettings($"users:{userID}", username);

            return $"{userID} - {username}";
        }

        public string GetUserIDWeb(string userIDOriginal)
        {
            if (long.TryParse(userIDOriginal, out _))
            {
                return _game.Alias.Equals("DS1") ? $"[U:1:{userIDOriginal}]" : userIDOriginal;
            }
            else if (long.TryParse(userIDOriginal, System.Globalization.NumberStyles.HexNumber, null, out long userID))
            {
                return userID.ToString();
            }
            else
            {
                throw new ArgumentException("Invalid userIDOriginal");
            }
        }


        public void OverwriteUsersAppSettings(string key, string value)
        {
            try
            {
                string? appsettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appSettings.json");
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
