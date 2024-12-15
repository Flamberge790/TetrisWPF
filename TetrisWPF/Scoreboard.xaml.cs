using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TetrisWPF
{
    /// <summary>
    /// Logika interakcji dla klasy Scoreboard.xaml
    /// </summary>
    public partial class Scoreboard : Page
    {
        public Scoreboard()
        {
            InitializeComponent();
            LoadScores();
        }

        private void ReturnToMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        public void LoadScores()
        {
            const int MaxVisibleScores = 10; // Target: Always fit 10 scores on screen
            const double ScorePanelHeight = 300; // Example: Total height for the scoreboard area

            double rowHeight = ScorePanelHeight / MaxVisibleScores; // Height for each score

            if (File.Exists("scoreboard.txt")) // Check if file exists
            {
                string[] scores = File.ReadAllLines("scoreboard.txt");

                // Clear previous items
                ScoreList.Children.Clear();

                // Display scores (limit to max visible)
                for (int i = 0; i < MaxVisibleScores; i++)
                {
                    string score = i < scores.Length ? scores[i] : ""; // Placeholder if fewer scores
                    TextBlock textBlock = new TextBlock
                    {
                        Text = !string.IsNullOrEmpty(score) ? score : " ",
                        FontSize = 12, // Font size inside rows
                        FontFamily = new FontFamily("eurofighter"),
                        Foreground = Brushes.White,
                        Margin = new Thickness(2),
                        Height = rowHeight, // Uniform height for each row
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.NoWrap // Single line for scores
                    };

                    ScoreList.Children.Add(textBlock);
                }
            }
            else
            {
                // No scores available
                ScoreList.Children.Clear();
                for (int i = 0; i < MaxVisibleScores; i++)
                {
                    TextBlock placeholder = new TextBlock
                    {
                        Text = i == 0 ? "No scores available" : " ",
                        FontSize = 12,
                        FontFamily = new FontFamily("eurofighter"),
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(5),
                        Height = rowHeight,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    };

                    ScoreList.Children.Add(placeholder);
                }
            }
        }


    }
}
