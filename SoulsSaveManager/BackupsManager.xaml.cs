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

            _userBackupPath = $"{_game.BackupPath}\\{_selectedUserComboBox}";
            _userSaveDataPath = $"{_game.SaveDataPath}\\{_selectedUserComboBox}";

            _utils.LoadBackupsComboBox(BackupsComboBox, _userBackupPath);

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
                    _utils.LoadBackupsComboBox(BackupsComboBox, _userBackupPath);
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

        private void DeleteBackup_Click(object sender, RoutedEventArgs e)
        {
            string nameBackup = BackupsComboBox.Text;
            string sourcePath = $"{_userBackupPath}\\{nameBackup}";
            Directory.Delete(sourcePath, true);
            MessageBox.Show("Done", "", MessageBoxButton.OK, MessageBoxImage.Information);
            _utils.LoadBackupsComboBox(BackupsComboBox, _userBackupPath);
        }

        public void UsersComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox? senderTyped = (System.Windows.Controls.ComboBox)sender;
            _selectedUserComboBox = senderTyped.SelectedValue.ToString()?.Split(" - ")[0] ?? "";
            _utils.LoadBackupsComboBox(BackupsComboBox, _userBackupPath);
        }
    }
}
