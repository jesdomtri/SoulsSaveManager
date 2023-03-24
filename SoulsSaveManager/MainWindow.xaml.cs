namespace SoulsSaveManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ClickSelectGame(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button senderTyped) return;

            Game game = new Game(senderTyped.Name);
            BackupsManager backupManager = new BackupsManager(this, game);
            backupManager.Show();
            Hide();
        }
    }
}
