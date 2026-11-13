using Microsoft.Extensions.DependencyInjection;
using SoulsSaveManager.Services;

namespace SoulsSaveManager
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        public MainWindow() : this(App.ServiceProvider ?? throw new InvalidOperationException("Service provider no inicializado."))
        {
        }

        private void ClickSelectGame(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button senderTyped) return;

            var game = new Game(senderTyped.Name);
            var pathResolver = _serviceProvider.GetRequiredService<IPathResolver>();
            var settingsStore = _serviceProvider.GetRequiredService<ISettingsStore>();
            var backupService = _serviceProvider.GetRequiredService<IBackupService>();
            var profileResolver = _serviceProvider.GetRequiredService<IUserProfileResolver>();
            var logger = _serviceProvider.GetRequiredService<IAppLogger>();

            var backupManager = new BackupsManager(this, game, pathResolver, settingsStore, backupService, profileResolver, logger);
            backupManager.Show();
            Hide();
        }
    }
}
