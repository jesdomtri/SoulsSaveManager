namespace SoulsSaveManager
{
    public partial class BackupsManager : Window
    {
        private MainWindow _mainWindow;
        private Game _game;
        private string _selectedUserComboBox;
        private string _userSaveDataPath;
        private string _userBackupPath;
        public BackupsManager(MainWindow mainWindow, Game game)
        {
            _mainWindow = mainWindow;
            _game = game;

            InitializeComponent();
            LoadUsersComboBox();

            _userBackupPath = $"{_game.BackupPath}\\{_selectedUserComboBox}";
            _userSaveDataPath = $"{_game.SaveDataPath}\\{_selectedUserComboBox}";

            LoadBackupsComboBox();

            _selectedUserComboBox = UsersComboBox.Text.Split(" - ")[0];
        }

        private void BackupManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mainWindow.Show();
        }

        private void OpenSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", _userSaveDataPath);
            }
            catch
            {
                MessageBox.Show("Something unexpected happened", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewBackup_Click(object sender, RoutedEventArgs e)
        {
            string nameBackup = NewBackupTextBox.Text;
            string targetPath = $"{_userBackupPath}\\{nameBackup}";
            if (string.IsNullOrEmpty(nameBackup))
            {
                MessageBox.Show("Backup's name is empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                    foreach (string? file in Directory.GetFiles(_userSaveDataPath))
                    {
                        string targetFile = $"{targetPath}\\{file.Split("\\").Last()}";
                        File.Copy(file, targetFile);
                    }
                    NewBackupTextBox.Clear();
                    MessageBox.Show("Done", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadBackupsComboBox();
                }
                else
                {
                    MessageBox.Show("There are already this backup", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadBackup_Click(object sender, RoutedEventArgs e)
        {
            string nameBackup = BackupsComboBox.Text;
            string sourcePath = $"{_userBackupPath}\\{nameBackup}";
            if (!Directory.Exists(_userSaveDataPath))
                Directory.CreateDirectory(_userSaveDataPath);
            foreach (string? file in Directory.GetFiles(sourcePath))
            {
                string targetFile = $"{_userSaveDataPath}\\{file.Split("\\").Last()}";
                File.Copy(file, targetFile, true);
            }
            MessageBox.Show("Done", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadUsersComboBox()
        {
            List<string>? listUsers = new List<string>();
            if (Directory.Exists(_game.SaveDataPath))
            {
                foreach (var user in Directory.EnumerateDirectories(_game.SaveDataPath))
                {
                    string userID = user.Split("\\").Last();
                    listUsers.Add(GetCompleteUser(userID));
                }
            }
            UsersComboBox.ItemsSource = listUsers;
        }

        private void LoadBackupsComboBox()
        {
            ObservableCollection<string>? listBackups = new ObservableCollection<string>();
            BackupsComboBox.ItemsSource = listBackups;
            try
            {
                if (Directory.Exists(_userBackupPath))
                {
                    foreach (string? backup in Directory.EnumerateDirectories(_userBackupPath))
                    {
                        listBackups.Add(backup.Split("\\").Last());
                    }
                }
            }
            catch
            {
                MessageBox.Show("Something unexpected happened", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            BackupsComboBox.ItemsSource = listBackups;
            BackupsComboBox.SelectedIndex = 0;
        }

        private string GetCompleteUser(string userID)
        {
            HtmlWeb oWeb = new HtmlWeb();
            HtmlDocument doc = oWeb.Load($"https://steamcommunity.com/profiles/{GetUserIDWeb(userID)}/");
            string username = doc.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("class", "").Contains("actual_persona_name")).ToList()[0].InnerHtml;
            return !string.IsNullOrEmpty(username) ? $"{userID} - {username}" : userID;
        }

        private string GetUserIDWeb(string userIDOriginal)
        {
            string userIDWeb = userIDOriginal;
            if (!long.TryParse(userIDOriginal, out _))
                userIDWeb = long.Parse(userIDOriginal, System.Globalization.NumberStyles.HexNumber).ToString();
            if (_game.Alias.Equals("DS1"))
                userIDWeb = $"[U:1:{userIDOriginal}]";
            return userIDWeb;
        }

        private void UsersComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox? senderTyped = (System.Windows.Controls.ComboBox)sender;
            _selectedUserComboBox = senderTyped.SelectedValue.ToString()?.Split(" - ")[0] ?? "";
            LoadBackupsComboBox();
        }
    }
}
