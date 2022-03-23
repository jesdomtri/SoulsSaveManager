namespace SoulsSaveManager
{
    public partial class MainWindow : Window
    {
        private BackupsManager backupManager;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ClickSelectGame(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button? senderTyped = (System.Windows.Controls.Button)sender;
            backupManager = new BackupsManager(this, senderTyped.Name);
            backupManager.Show();
            Hide();
        }
    }
}
