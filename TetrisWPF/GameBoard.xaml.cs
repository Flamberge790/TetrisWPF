using System;
using System.Windows;
using System.Windows.Controls;
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
        }

        private void GameBoard_Loaded(object sender, RoutedEventArgs e)
        {
            TetrisModel.StartNewGame("Player");

            GameCanvas.Width = TetrisModel.wellWidth * CellSize;
            GameCanvas.Height = TetrisModel.wellHeight * CellSize;

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(300);
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            Keyboard.Focus(this);

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
                GameOverText.Visibility = Visibility.Visible;
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
                        DrawCell(x, y, Brushes.Cyan);
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
                        if (boardY >= 0 && boardY < TetrisModel.wellHeight)
                        {
                            DrawCell(boardX, boardY, Brushes.Yellow);
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
                    Width = dim * (CellSize),
                    Height = dim * (CellSize),
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
                                Fill = Brushes.Yellow,
                                Stroke = Brushes.Gray,
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

        private void DrawCell(int x, int y, Brush color)
        {
            Rectangle rect = new Rectangle()
            {
                Width = CellSize,
                Height = CellSize,
                Fill = color,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };
            Canvas.SetLeft(rect, x * CellSize);
            int invertedY = (TetrisModel.wellHeight - 1 - y);
            Canvas.SetTop(rect, invertedY * CellSize);
            GameCanvas.Children.Add(rect);
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (TetrisModel.gameEnded) return;

            TetrisModel.HandleKey(e.Key);
            Redraw();
        }
    }
}
