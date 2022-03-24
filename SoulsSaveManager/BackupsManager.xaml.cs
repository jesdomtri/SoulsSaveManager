using System.Collections.Generic;
using System.IO;
using System;
using HtmlAgilityPack;
using System.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace SoulsSaveManager
{
    public partial class BackupsManager : Window
    {
        private MainWindow _mainWindow;
        private string _gameName;
        public BackupsManager(MainWindow mainWindow, string gameSelected)
        {
            _mainWindow = mainWindow;
            _gameName = SelectTitleGame(gameSelected);
            InitializeComponent();
            LoadUsersComboBox();
            LoadBackupsComboBox();
        }

        private void BackupManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mainWindow.Show();
        }

        private void OpenSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string user = UsersComboBox.Text.Split(" - ")[0];
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", $"\\Users\\{Environment.UserName}\\AppData\\Roaming\\{_gameName}\\{user}");
            }
            catch
            {
                MessageBox.Show("Something unexpected happened", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewBackup_Click(object sender, RoutedEventArgs e)
        {
            string nameBackup = NewBackupTextBox.Text;
            string user = UsersComboBox.Text.Split(" - ")[0];
            string sourcePath = $"\\Users\\{Environment.UserName}\\AppData\\Roaming\\{_gameName}\\{user}";
            string targetPath = $".\\{_gameName}\\{user}\\{nameBackup}";
            if (string.IsNullOrEmpty(nameBackup))
            {
                MessageBox.Show("Backup's name is empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            string user = UsersComboBox.Text.Split(" - ")[0];
            string sourcePath = $".\\{_gameName}\\{user}\\{BackupsComboBox.Text}";
            string targetPath = $"\\Users\\{Environment.UserName}\\AppData\\Roaming\\{_gameName}\\{user}";
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);
            foreach (string? file in Directory.GetFiles(sourcePath))
            {
                string targetFile = $"{targetPath}\\{file.Split("\\").Last()}";
                File.Copy(file, targetFile, true);
            }
            MessageBox.Show("Done", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadUsersComboBox()
        {
            List<string>? listUsers = new List<string>();
            try
            {
                foreach (var user in Directory.EnumerateDirectories($"/Users/{Environment.UserName}/AppData/Roaming/{_gameName}"))
                {
                    string userID = user.Split("\\")[1];
                    listUsers.Add(GetCompleteUser(userID));
                }
            }
            catch
            {
                MessageBox.Show("Something unexpected happened", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UsersComboBox.ItemsSource = listUsers;
        }

        private void LoadBackupsComboBox(string userID = "")
        {
            ObservableCollection<string>? listBackups = new ObservableCollection<string>();
            BackupsComboBox.ItemsSource = listBackups;
            string user = string.IsNullOrEmpty(userID) ? UsersComboBox.Text.Split(" - ")[0] : userID;
            string path = $".\\{_gameName}\\{user}";
            try
            {
                if (Directory.Exists(path))
                {
                    foreach (string? backup in Directory.EnumerateDirectories(path))
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
            if (_gameName.Equals("DS1"))
                userIDWeb = $"[U:1:{userIDOriginal}]";
            return userIDWeb;
        }

        private static string SelectTitleGame(string gameSelected)
        {
            switch (gameSelected)
            {
                case "DS1":
                    return "DarkSoulsRemastered";
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

        private void UsersComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox? senderTyped = (System.Windows.Controls.ComboBox)sender;
            LoadBackupsComboBox(senderTyped.SelectedValue.ToString()?.Split(" - ")[0] ?? "");
        }
    }
}
