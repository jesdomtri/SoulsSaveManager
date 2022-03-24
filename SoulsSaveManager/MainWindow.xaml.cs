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
            System.Windows.Controls.Button? senderTyped = (System.Windows.Controls.Button)sender;
            Game game = new Game(senderTyped.Name);
            BackupsManager backupManager = new BackupsManager(this, game);
            backupManager.Show();
            Hide();
        }
    }
}
