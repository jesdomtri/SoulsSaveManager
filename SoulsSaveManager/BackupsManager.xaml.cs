using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using SoulsSaveManager.Services;

namespace SoulsSaveManager
{
    public partial class BackupsManager : Window
    {
        private readonly MainWindow _mainWindow;
        private readonly Game _game;
        private readonly IPathResolver _pathResolver;
        private readonly ISettingsStore _settingsStore;
        private readonly IBackupService _backupService;
        private readonly IUserProfileResolver _profileResolver;
        private readonly IAppLogger _logger;

        private string _selectedUserId = string.Empty;
        private string _userSaveDataPath = string.Empty;
        private string _userBackupPath = string.Empty;

        public BackupsManager(MainWindow mainWindow, Game game) : this(
            mainWindow,
            game,
            App.ServiceProvider?.GetService(typeof(IPathResolver)) as IPathResolver ?? new PathResolver(),
            App.ServiceProvider?.GetService(typeof(ISettingsStore)) as ISettingsStore ?? new JsonSettingsStore(),
            App.ServiceProvider?.GetService(typeof(IBackupService)) as IBackupService ?? new BackupService(),
            App.ServiceProvider?.GetService(typeof(IUserProfileResolver)) as IUserProfileResolver ?? new UserProfileResolver(App.ServiceProvider?.GetService(typeof(ISettingsStore)) as ISettingsStore ?? new JsonSettingsStore()),
            App.ServiceProvider?.GetService(typeof(IAppLogger)) as IAppLogger ?? new FileAppLogger())
        {
        }

        public BackupsManager(
            MainWindow mainWindow,
            Game game,
            IPathResolver pathResolver,
            ISettingsStore settingsStore,
            IBackupService backupService,
            IUserProfileResolver profileResolver,
            IAppLogger logger)
        {
            _mainWindow = mainWindow;
            _game = game;
            _pathResolver = pathResolver;
            _settingsStore = settingsStore;
            _backupService = backupService;
            _profileResolver = profileResolver;
            _logger = logger;

            InitializeComponent();
            Loaded += (_, _) => Initialize();
        }

        private void Initialize()
        {
            try
            {
                var settings = _settingsStore.Load();
                if (_game.Alias == "DM")
                {
                    UsersComboBox.IsEnabled = false;
                    var dmLocation = settings.Locations.TryGetValue("DM", out var loc) ? loc : string.Empty;
                    _selectedUserId = string.Empty;
                    _userSaveDataPath = dmLocation ?? string.Empty;
                    SaveLocationTextBox.Text = _userSaveDataPath;
                    UsersComboBox.ItemsSource = new ObservableCollection<string> { _profileResolver.GetDisplayLabel("", "DM") };
                    UsersComboBox.SelectedIndex = 0;
                }
                else
                {
                    var saveRoot = _pathResolver.GetDefaultSaveRoot(_game.Alias);
                    var profileIds = _profileResolver.DiscoverProfiles(_game.Alias, saveRoot);
                    var labels = profileIds.Select(id => _profileResolver.GetDisplayLabel(id, _game.Alias)).ToList();
                    UsersComboBox.ItemsSource = new ObservableCollection<string>(labels);
                    if (UsersComboBox.HasItems) UsersComboBox.SelectedIndex = 0;
                    _selectedUserId = ExtractUserId(UsersComboBox.SelectedValue?.ToString());
                    _userSaveDataPath = _pathResolver.GetSavePath(_game.Alias, _selectedUserId);
                    SaveLocationTextBox.Text = _userSaveDataPath;

                    // resolve labels asynchronously without blocking UI
                    _ = Task.Run(async () =>
                    {
                        foreach (var id in profileIds)
                        {
                            try
                            {
                                var resolved = await _profileResolver.ResolveLabelAsync(id, _game.Alias);
                                await Dispatcher.InvokeAsync(() => UpdateUserLabel(id, resolved));
                            }
                            catch { /* best effort */ }
                        }
                    });
                }

                RefreshPathsAndBackups();
            }
            catch (Exception ex)
            {
                ShowError("Error al inicializar la gestión de backups.", ex);
            }
        }

        private void UpdateUserLabel(string userId, string resolvedLabel)
        {
            if (UsersComboBox.ItemsSource is not ObservableCollection<string> items) return;
            var idx = items.ToList().FindIndex(x => ExtractUserId(x) == userId);
            if (idx >= 0)
            {
                var selected = ExtractUserId(UsersComboBox.SelectedValue?.ToString());
                items[idx] = resolvedLabel;
                if (selected == userId) UsersComboBox.SelectedIndex = idx;
            }
        }

        private string ExtractUserId(string? label)
        {
            if (string.IsNullOrWhiteSpace(label)) return string.Empty;
            if (_game.Alias == "DM") return string.Empty;
            var parts = label.Split(" - ", 2, StringSplitOptions.None);
            return parts[0];
        }

        private IEnumerable<string> GetLegacyBackupPaths()
        {
            return _pathResolver.GetLegacyBackupPaths(_game.Alias, _selectedUserId);
        }

        private string GetConfiguredBackupRoot()
        {
            var settings = _settingsStore.Load();
            return string.IsNullOrWhiteSpace(settings.BackupRoot) ? _pathResolver.GetDefaultBackupRoot() : settings.BackupRoot!;
        }

        private void RefreshPathsAndBackups()
        {
            if (_game.Alias == "DM")
            {
                var settings = _settingsStore.Load();
                var dm = settings.Locations.TryGetValue("DM", out var loc) ? loc : string.Empty;
                if (!string.IsNullOrWhiteSpace(SaveLocationTextBox.Text))
                    _userSaveDataPath = SaveLocationTextBox.Text;
                else
                    _userSaveDataPath = dm ?? string.Empty;
                _userBackupPath = _pathResolver.GetBackupPath(_game.Alias, string.Empty, GetConfiguredBackupRoot());
            }
            else
            {
                _selectedUserId = ExtractUserId(UsersComboBox.SelectedValue?.ToString());
                if (string.IsNullOrWhiteSpace(SaveLocationTextBox.Text) || SaveLocationTextBox.Text == _pathResolver.GetSavePath(_game.Alias, _selectedUserId))
                {
                    _userSaveDataPath = _pathResolver.GetSavePath(_game.Alias, _selectedUserId);
                    SaveLocationTextBox.Text = _userSaveDataPath;
                }
                else
                {
                    _userSaveDataPath = SaveLocationTextBox.Text;
                }
                _userBackupPath = _pathResolver.GetBackupPath(_game.Alias, _selectedUserId, GetConfiguredBackupRoot());
            }

            LoadBackups();
        }

        private void LoadBackups()
        {
            try
            {
                var backups = _backupService.ListBackups(_userBackupPath, GetLegacyBackupPaths());
                BackupsComboBox.ItemsSource = new ObservableCollection<string>(backups);
                if (BackupsComboBox.HasItems) BackupsComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError("No se pudieron listar los backups.", ex);
            }
        }

        private void BackupManager_Closing(object? sender, CancelEventArgs e)
        {
            _mainWindow.Show();
        }

        private void OpenSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            var savePath = (SaveLocationTextBox.Text ?? string.Empty).Trim();
            try
            {
                if (!string.IsNullOrWhiteSpace(savePath) && Directory.Exists(savePath))
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", savePath) { UseShellExecute = true });
                }
                else
                {
                    System.Windows.MessageBox.Show("La ubicación de guardado no existe.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ShowError("No se pudo abrir la ubicación de guardado.", ex);
            }
        }

        private void OpenBackupLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(_userBackupPath);
                Process.Start(new ProcessStartInfo("explorer.exe", _userBackupPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ShowError("No se pudo abrir la ubicación de backups.", ex);
            }
        }

        private void SelectSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var dialog = new FolderBrowserDialog();
                var result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    SaveLocationTextBox.Text = dialog.SelectedPath;
                    _userSaveDataPath = dialog.SelectedPath;
                    if (_game.Alias == "DM")
                    {
                        var settings = _settingsStore.Load();
                        settings.Locations["DM"] = dialog.SelectedPath;
                        _settingsStore.Save(settings);
                    }
                    RefreshPathsAndBackups();
                }
            }
            catch (Exception ex)
            {
                ShowError("No se pudo seleccionar la ubicación de guardado.", ex);
            }
        }

        private void ResetSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_game.Alias == "DM")
                {
                    var settings = _settingsStore.Load();
                    settings.Locations["DM"] = string.Empty;
                    _settingsStore.Save(settings);
                    SaveLocationTextBox.Text = string.Empty;
                    _userSaveDataPath = string.Empty;
                }
                else
                {
                    var defaultPath = _pathResolver.GetSavePath(_game.Alias, _selectedUserId);
                    SaveLocationTextBox.Text = defaultPath;
                    _userSaveDataPath = defaultPath;
                }
                RefreshPathsAndBackups();
            }
            catch (Exception ex)
            {
                ShowError("No se pudo restablecer la ubicación de guardado.", ex);
            }
        }

        private void NewBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nameBackup = (NewBackupTextBox.Text ?? string.Empty).Trim();
                var sourcePath = (SaveLocationTextBox.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(sourcePath))
                {
                    sourcePath = _userSaveDataPath;
                }

                _backupService.CreateBackup(sourcePath, _userBackupPath, nameBackup);
                NewBackupTextBox.Clear();
                System.Windows.MessageBox.Show("Backup creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadBackups();
            }
            catch (Exception ex)
            {
                ShowError("No se pudo crear el backup.", ex);
            }
        }

        private void LoadBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nameBackup = (BackupsComboBox.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(nameBackup))
                {
                    System.Windows.MessageBox.Show("Selecciona un backup para cargar.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var targetPath = (SaveLocationTextBox.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(targetPath)) targetPath = _userSaveDataPath;

                var confirm = System.Windows.MessageBox.Show(
                    "Esto sobrescribirá los archivos de guardado actuales en la ubicación seleccionada. ¿Deseas continuar?",
                    "Confirmar restauración",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return;

                _backupService.RestoreBackup(_userBackupPath, nameBackup, targetPath, GetLegacyBackupPaths());
                System.Windows.MessageBox.Show("Backup restaurado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError("No se pudo restaurar el backup.", ex);
            }
        }

        private void DeleteBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nameBackup = (BackupsComboBox.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(nameBackup))
                {
                    System.Windows.MessageBox.Show("Selecciona un backup para eliminar.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var confirm = System.Windows.MessageBox.Show(
                    $"¿Eliminar el backup '{nameBackup}'? Esta acción no se puede deshacer.",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return;

                _backupService.DeleteBackup(_userBackupPath, nameBackup, GetLegacyBackupPaths());
                System.Windows.MessageBox.Show("Backup eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadBackups();
            }
            catch (Exception ex)
            {
                ShowError("No se pudo eliminar el backup.", ex);
            }
        }

        private void UsersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            try
            {
                _selectedUserId = ExtractUserId((sender as System.Windows.Controls.ComboBox)?.SelectedValue?.ToString());
                if (_game.Alias != "DM")
                {
                    SaveLocationTextBox.Text = _pathResolver.GetSavePath(_game.Alias, _selectedUserId);
                }
                RefreshPathsAndBackups();
            }
            catch (Exception ex)
            {
                ShowError("No se pudo cambiar el perfil seleccionado.", ex);
            }
        }

        private void ShowError(string message, Exception ex)
        {
            try { _logger.Error(message, ex); } catch { }
            System.Windows.MessageBox.Show($"{message}\n\nDetalle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
