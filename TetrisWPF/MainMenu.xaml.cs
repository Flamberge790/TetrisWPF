using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TetrisWPF
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        // Obsługa przycisku START
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GameBoard());
        }

        // Obsługa przycisku SCOREBOARD
        private void ScoreboardButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Scoreboard());
        }

        // Obsługa przycisku EXIT
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
