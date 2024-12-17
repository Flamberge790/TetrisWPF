using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TetrisWPF
{
    public partial class GameBoard : Page
    {
        private const int CellSize = 25;
        private DispatcherTimer gameTimer;

        public GameBoard()
        {
            InitializeComponent();
            this.Focusable = true;
            this.Focus();
            Loaded += GameBoard_Loaded;
            gameTimer = new DispatcherTimer();
        }

        private void GameBoard_Loaded(object sender, RoutedEventArgs e)
        {
            EnterNamePanel.Visibility = Visibility.Visible;
            GameCanvas.Width = TetrisModel.wellWidth * CellSize;
            GameCanvas.Height = TetrisModel.wellHeight * CellSize;
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PlayerNameTextBox.Text))
            {
                TetrisModel.playerName = PlayerNameTextBox.Text;
                EnterNamePanel.Visibility = Visibility.Collapsed;
                GameCanvas.Visibility = Visibility.Visible;
                NextPiecesCanvas.Visibility = Visibility.Visible;
                StartGame();
            }
            else
            {
                MessageBox.Show("Please enter a valid name!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StartGame()
        {
            TetrisModel.StartNewGame(TetrisModel.playerName);
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(1000);
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
            Redraw();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!TetrisModel.gameEnded)
            {
                TetrisModel.HandleKey(Key.Down);
                Redraw();
            }
            else
            {
                gameTimer.Stop();
                TetrisModel.SaveScore();
                FinalScoreText.Text = $"SCORE: {TetrisModel.score}";
                GameOverPanel.Visibility = Visibility.Visible;
            }
        }

        private void Redraw()
        {
            GameCanvas.Children.Clear();

            for (int y = 0; y < TetrisModel.wellHeight; y++)
            {
                for (int x = 0; x < TetrisModel.wellWidth; x++)
                {
                    if (TetrisModel.well[y, x] == 1)
                    {
                        DrawCell(x, y, Brushes.BlueViolet, Brushes.Black, 1);
                    }
                }
            }

            int pieceIndex = TetrisModel.pieceNo;
            int rotation = TetrisModel.pieceRotate;
            int[,,] currentPiece = TetrisModel.pieces[pieceIndex];
            int dim = currentPiece.GetLength(1);

            for (int py = 0; py < dim; py++)
            {
                for (int px = 0; px < dim; px++)
                {
                    if (currentPiece[rotation, py, px] == 1)
                    {
                        int boardX = TetrisModel.pieceX + px;
                        int boardY = TetrisModel.pieceY - py;

                        if (boardY >= TetrisModel.wellHeight || boardY < 0)
                        {
                            TetrisModel.gameEnded = true;
                            FinalScoreText.Text = $"SCORE: {TetrisModel.score}";
                            GameOverPanel.Visibility = Visibility.Visible;
                            gameTimer.Stop();
                            return;
                        }

                        if (boardY >= 0 && boardY < TetrisModel.wellHeight)
                        {
                            DrawCell(boardX, boardY, Brushes.Black, Brushes.Gray, 1);
                        }
                    }
                }
            }

            ScoreText.Text = $"SCORE: {TetrisModel.score}";
            ShowUpcomingPieces();
        }

        private void ShowUpcomingPieces()
        {
            NextPiecesPanel.Children.Clear();

            TextBlock nextLabel = new TextBlock
            {
                Text = "NEXT:",
                FontSize = 20,
                FontFamily = new FontFamily("eurofighter"),
                Foreground = Brushes.White,
                Margin = new Thickness(20, 0, 0, 25)
            };
            NextPiecesPanel.Children.Add(nextLabel);

            int piecesToShow = Math.Min(TetrisModel.upcomingPieces.Count, 5);
            for (int i = 0; i < piecesToShow; i++)
            {
                int pieceId = TetrisModel.upcomingPieces[i];
                var pieceData = TetrisModel.pieces[pieceId];
                int dim = pieceData.GetLength(1);

                Canvas previewCanvas = new Canvas
                {
                    Width = 4 * (CellSize),
                    Height = 4 * (CellSize),
                    Margin = new Thickness(0, 0, 0, 25)
                };

                for (int py = 0; py < dim; py++)
                {
                    for (int px = 0; px < dim; px++)
                    {
                        if (pieceData[0, py, px] == 1)
                        {
                            Rectangle cell = new Rectangle
                            {
                                Width = CellSize,
                                Height = CellSize,
                                Fill = Brushes.DarkTurquoise,
                                Stroke = Brushes.Black,
                                StrokeThickness = 0.5
                            };
                            Canvas.SetLeft(cell, px * (CellSize));
                            Canvas.SetTop(cell, py * (CellSize));
                            previewCanvas.Children.Add(cell);
                        }
                    }
                }
                NextPiecesPanel.Children.Add(previewCanvas);
            }
        }

        private void DrawCell(int x, int y, Brush color, Brush stroke, double strokethickness)
        {
            Rectangle rect = new Rectangle()
            {
                Width = CellSize - 0.5,
                Height = CellSize - 0.5,
                Fill = color,
                Stroke = stroke,
                StrokeThickness = strokethickness
            };
            Canvas.SetLeft(rect, x * CellSize);
            int invertedY = (TetrisModel.wellHeight - 1 - y);
            Canvas.SetTop(rect, invertedY * CellSize);
            GameCanvas.Children.Add(rect);
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (TetrisModel.gameEnded || EnterNamePanel.Visibility == Visibility.Visible) return;

            TetrisModel.HandleKey(e.Key);
            Redraw();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            TetrisModel.StartNewGame(TetrisModel.playerName);
            GameOverPanel.Visibility = Visibility.Collapsed;
            gameTimer.Start();
            Redraw();
        }

        private void ReturnToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            TetrisModel.playerName = ""; // Wyczyszczenie nicku
            GameOverPanel.Visibility = Visibility.Collapsed;
            NavigationService.Navigate(new MainMenu());
        }
    }
}
