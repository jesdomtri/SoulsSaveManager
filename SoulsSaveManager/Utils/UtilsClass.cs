namespace SoulsSaveManager.Utils
{
    public class UtilsClass
    {
        private Game _game;
        private List<string>? _listUsers;
        private bool _finishedThread;
        public UtilsClass(Game game)
        {
            _game = game;
        }

        public void LoadUsersComboBox(ref System.Windows.Controls.ComboBox usersComboBox)
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerAsync();
            while (backgroundWorker.IsBusy)
            {
                if (_finishedThread)
                {
                    backgroundWorker.Dispose();
                    break;
                }
            }
            usersComboBox.ItemsSource = _listUsers;
        }

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            _listUsers = new List<string>();
            if (Directory.Exists(_game.SaveDataPath))
            {
                foreach (var user in Directory.EnumerateDirectories(_game.SaveDataPath))
                {
                    string userID = user.Split("\\").Last();
                    _listUsers.Add(GetCompleteUser(userID));
                }
            }
            _finishedThread = true;
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
                MessageBox.Show("Something unexpected happened", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            backupsComboBox.ItemsSource = listBackups;
            backupsComboBox.SelectedIndex = 0;
        }

        public string GetCompleteUser(string userID)
        {
            HtmlWeb oWeb = new HtmlWeb();
            HtmlDocument doc = oWeb.Load($"https://steamcommunity.com/profiles/{GetUserIDWeb(userID)}/");
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
