using System.Threading.Tasks;
using System.Windows.Controls;

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
            SaveLocationTextBox.Text = _userSaveDataPath;

            if (_game.Alias.Equals("DM"))
            {
                UsersComboBox.IsEnabled = false;
                SaveLocationTextBox.Text = App.LocationsCache!["DM"];
            }
            
            _selectedUserComboBox = UsersComboBox.Text.Split(" - ")[0];

            _userBackupPath = Path.Combine(_game.BackupPath, _selectedUserComboBox!);
            _userSaveDataPath = Path.Combine(_game.SaveDataPath, _selectedUserComboBox!);

            UtilsClass.LoadBackupsComboBox(BackupsComboBox, _userBackupPath);
        }

        private void BackupManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mainWindow.Show();
        }

        private void OpenSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            string savePath = SaveLocationTextBox.Text.Trim();
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = _userSaveDataPath;
            }
            try
            {
                if (Directory.Exists(savePath))
                {
                    Process.Start("explorer.exe", savePath);
                }
                else
                {
                    System.Windows.MessageBox.Show("This location doesn't exist", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Something unexpected happened: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenBackupLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(_userBackupPath))
                {
                    var explorerPath = Path.Combine(Environment.GetEnvironmentVariable("WINDIR")!, "explorer.exe");
                    Process.Start(explorerPath, _userBackupPath);
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

        private async void SelectSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                SaveLocationTextBox.Text = dialog.SelectedPath;
                _userSaveDataPath = dialog.SelectedPath;
                if (_game.Alias.Equals("DM"))
                    _utils.OverwriteUsersAppSettings($"locations:{_game.Alias}", dialog.SelectedPath);
            }
            await Task.CompletedTask;
        }

        private async void ResetSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            if (!_game.Alias.Equals("DM"))
            {
                _userSaveDataPath = Path.Combine(_game.SaveDataPath, _selectedUserComboBox);
                SaveLocationTextBox.Text = Path.Combine(_game.SaveDataPath, _selectedUserComboBox);
            }
            else
            {
                _userSaveDataPath = "";
                SaveLocationTextBox.Clear();
            }
            await Task.CompletedTask;
        }

        private void NewBackup_Click(object sender, RoutedEventArgs e)
        {
            var nameBackup = NewBackupTextBox.Text.Trim();
            var targetPath = Path.Combine(_userBackupPath, nameBackup);
            var sourcePath = string.IsNullOrWhiteSpace(SaveLocationTextBox.Text) ? targetPath : SaveLocationTextBox.Text;
            if (!Directory.Exists(sourcePath))
            {
                System.Windows.MessageBox.Show("The save location doesn't exist", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(nameBackup))
            {
                System.Windows.MessageBox.Show("Backup's name is empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Directory.Exists(targetPath))
            {
                System.Windows.MessageBox.Show("This backup already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Directory.CreateDirectory(targetPath);
            var files = Directory.GetFiles(sourcePath);
            foreach (var file in files)
            {
                var targetFile = Path.Combine(targetPath, Path.GetFileName(file));
                File.Copy(file, targetFile);
            }

            NewBackupTextBox.Clear();
            System.Windows.MessageBox.Show("Done", "", MessageBoxButton.OK, MessageBoxImage.Information);
            UtilsClass.LoadBackupsComboBox(BackupsComboBox, _userBackupPath);
        }

        private void LoadBackup_Click(object sender, RoutedEventArgs e)
        {
            var nameBackup = BackupsComboBox.Text.Trim();
            var sourcePath = Path.Combine(_userBackupPath, nameBackup);
            var targetPath = string.IsNullOrWhiteSpace(SaveLocationTextBox.Text) ? _userSaveDataPath : SaveLocationTextBox.Text;
            Directory.CreateDirectory(targetPath);
            var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var targetFile = Path.Combine(targetPath, Path.GetFileName(file));
                File.Copy(file, targetFile, true);
            }

            System.Windows.MessageBox.Show("Done", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DeleteBackup_Click(object sender, RoutedEventArgs e)
        {
            string nameBackup = BackupsComboBox.Text;
            string sourcePath = Path.Combine(_userBackupPath, nameBackup);
            try
            {
                await Task.Run(() => Directory.Delete(sourcePath, true));
                System.Windows.MessageBox.Show("Done", "", MessageBoxButton.OK, MessageBoxImage.Information);
                UtilsClass.LoadBackupsComboBox(BackupsComboBox, _userBackupPath);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error deleting backup: {ex.Message}", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UsersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var senderTyped = (System.Windows.Controls.ComboBox)sender;
            _selectedUserComboBox = senderTyped?.SelectedValue?.ToString()?.Split(" - ")[0] ?? "";
            _userBackupPath = Path.Combine(_game.BackupPath, _selectedUserComboBox);
            _userSaveDataPath = Path.Combine(_game.SaveDataPath, _selectedUserComboBox);
            SaveLocationTextBox.Text = _userSaveDataPath;
            UtilsClass.LoadBackupsComboBox(BackupsComboBox, _userBackupPath);
        }
    }
}
