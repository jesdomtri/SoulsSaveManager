namespace SoulsSaveManager
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }
        public static IConfiguration? Configuration { get; private set; }
        public static Dictionary<string, string>? UsersCache = new();
        public static Dictionary<string, string>? LocationsCache = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            IConfigurationBuilder? builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
            
            UsersCache = Configuration?.GetSection("users").Get<Dictionary<string, string>>();
            LocationsCache = Configuration?.GetSection("locations").Get<Dictionary<string, string>>();

            ServiceCollection? serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            MainWindow? mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(typeof(MainWindow));
        }
    }
}
