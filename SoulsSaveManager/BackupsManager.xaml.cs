namespace SoulsSaveManager
{
    public partial class BackupsManager : Window
    {
        private MainWindow _mainWindow;
        private Game _game;
        private UtilsClass _utils;
        private string _selectedUserComboBox;
        private string _userSaveDataPath;
        private string _userBackupPath;
        public BackupsManager(MainWindow mainWindow, Game game)
        {
            _mainWindow = mainWindow;
            _game = game;
            _utils = new UtilsClass(game);

            InitializeComponent();

            UsersComboBox.ItemsSource = _utils.LoadUsersComboBox();
            if (_game.Alias.Equals("DM"))
                UsersComboBox.IsEnabled = false;

            _userBackupPath = $"{_game.BackupPath}\\{_selectedUserComboBox}";
            _userSaveDataPath = $"{_game.SaveDataPath}\\{_selectedUserComboBox}";

            SaveLocationTextBox.Text = _game.Alias.Equals("DM") ? App.LocationsCache["DM"] : _userSaveDataPath;

            _utils.LoadBackupsComboBox(BackupsComboBox, _userBackupPath);

            _selectedUserComboBox = UsersComboBox.Text.Split(" - ")[0];
        }

        private void BackupManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mainWindow.Show();
        }

        private void OpenSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            string savePath = string.IsNullOrEmpty(SaveLocationTextBox.Text) ? _userSaveDataPath : SaveLocationTextBox.Text;
            try
            {
                if (Directory.Exists(savePath))
                {
                    Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", savePath);
                }
                else
                {
                    System.Windows.MessageBox.Show("This location doesn't exist", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("Something unexpected happened", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenBackupLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(_userBackupPath))
                {
                    Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", _userBackupPath);
                }
                else
                {
                    System.Windows.MessageBox.Show("This location doesn't exist", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("Something unexpected happened", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    SaveLocationTextBox.Text = fbd.SelectedPath;
                    _userSaveDataPath = fbd.SelectedPath;
                    if (_game.Alias.Equals("DM"))
                        _utils.OverwriteUsersAppSettings($"locations:{_game.Alias}", fbd.SelectedPath);
                }
            }
        }

        private void ResetSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            if (!_game.Alias.Equals("DM"))
            {
                _userSaveDataPath = $"{_game.SaveDataPath}\\{_selectedUserComboBox}";
                SaveLocationTextBox.Text = $"{_game.SaveDataPath}\\{_selectedUserComboBox}";
            }
            else
            {
                _userSaveDataPath = "";
                SaveLocationTextBox.Clear();
            }
        }

        private void NewBackup_Click(object sender, RoutedEventArgs e)
        {
            string nameBackup = NewBackupTextBox.Text;
            string targetPath = $"{_userBackupPath}\\{nameBackup}";
            string sourcePath = string.IsNullOrEmpty(SaveLocationTextBox.Text) ? $"{_userBackupPath}\\{nameBackup}" : SaveLocationTextBox.Text;
            if (Directory.Exists(sourcePath))
            {
                if (string.IsNullOrEmpty(nameBackup))
                {
                    System.Windows.MessageBox.Show("Backup's name is empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                        foreach (string? file in Directory.GetFiles(sourcePath))
                        {
                            string targetFile = $"{targetPath}\\{file.Split("\\").Last()}";
                            File.Copy(file, targetFile);
                        }
                        NewBackupTextBox.Clear();
                        System.Windows.MessageBox.Show("Done", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        _utils.LoadBackupsComboBox(BackupsComboBox, _userBackupPath);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("This backup already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("The save location doesn't exist", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadBackup_Click(object sender, RoutedEventArgs e)
        {
            string nameBackup = BackupsComboBox.Text;
            string sourcePath = $"{_userBackupPath}\\{nameBackup}";
            if (!Directory.Exists(_userSaveDataPath))
            {
                Directory.CreateDirectory(_userSaveDataPath);
            }
            else
            {
                Directory.Delete(_userSaveDataPath, true);
                Directory.CreateDirectory(_userSaveDataPath);
            }
            foreach (string? file in Directory.GetFiles(sourcePath))
            {
                string targetFile = $"{_userSaveDataPath}\\{file.Split("\\").Last()}";
                File.Copy(file, targetFile, true);
            }
            System.Windows.MessageBox.Show("Done", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteBackup_Click(object sender, RoutedEventArgs e)
        {
            string nameBackup = BackupsComboBox.Text;
            string sourcePath = $"{_userBackupPath}\\{nameBackup}";
            Directory.Delete(sourcePath, true);
            System.Windows.MessageBox.Show("Done", "", MessageBoxButton.OK, MessageBoxImage.Information);
            _utils.LoadBackupsComboBox(BackupsComboBox, _userBackupPath);
        }

        public void UsersComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox? senderTyped = (System.Windows.Controls.ComboBox)sender;
            _selectedUserComboBox = senderTyped.SelectedValue.ToString()?.Split(" - ")[0] ?? "";
            _userBackupPath = $"{_game.BackupPath}\\{_selectedUserComboBox}";
            _userSaveDataPath = $"{_game.SaveDataPath}\\{_selectedUserComboBox}";
            SaveLocationTextBox.Text = _userSaveDataPath;
            _utils.LoadBackupsComboBox(BackupsComboBox, _userBackupPath);
        }
    }
}
