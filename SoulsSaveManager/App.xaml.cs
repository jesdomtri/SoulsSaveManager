using SoulsSaveManager.Services;

namespace SoulsSaveManager
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }
        public static ISettingsStore? SettingsStore { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var settingsStore = new JsonSettingsStore();
            SettingsStore = settingsStore;
            settingsStore.Load(); // ensures canonical settings/migration

            var services = new ServiceCollection();
            ConfigureServices(services, settingsStore);
            ServiceProvider = services.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection services, ISettingsStore settingsStore)
        {
            services.AddSingleton(settingsStore);
            services.AddSingleton<IAppLogger, FileAppLogger>();
            services.AddSingleton<IPathResolver, PathResolver>();
            services.AddSingleton<IBackupService, BackupService>();
            services.AddSingleton<IUserProfileResolver>(sp => new UserProfileResolver(sp.GetRequiredService<ISettingsStore>()));
            services.AddTransient<MainWindow>();
            services.AddTransient<BackupsManager>();
        }
    }
}
