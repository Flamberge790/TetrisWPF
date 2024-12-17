using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TetrisWPF
{
    public class TetrisModel
    {
        // block
        private static object syncObject = new object();
        public static int pieceYinit = 24;
        public static int pieceNo = 4;
        public static int pieceRotate = 1;
        public static int pieceX = 4;
        public static int pieceY = pieceYinit;
        private static int bagIdx = -1;
        public static int[] currentBag = new int[] { 0, 1, 2, 3, 4, 5, 6 };
        public static List<int> upcomingPieces = new List<int>();

        // gamestate
        public static Random rnd = new Random();
        internal static DispatcherTimer? timer;    // Zmiana: użycie DispatcherTimer dla WPF
        static int dropInterval = 1000;
        public static bool gameEnded = false;
        public static bool keyPressedRecently = false;

        // gameboard
        public static int wellWidth = 10;
        public static int wellHeight = 25;
        public static int[,] well = new int[wellHeight, wellWidth];

        // player
        public static string playerName = "";
        public static int score = 0;

        public static Action? OnRedrawRequested;
        public static Action? OnGameOver;

        public static readonly List<Brush> pieceColors = new List<Brush>
        {
            Brushes.Cyan,       // I
            Brushes.Blue,       // J
            Brushes.Orange,     // L
            Brushes.Yellow,     // O
            Brushes.LawnGreen,  // S
            Brushes.DarkViolet, // T
            Brushes.Red         // Z
        };

        public static readonly List<int[,,]> pieces = new List<int[,,]> {
            // I
            new int[,,] {
                { {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} },
                { {0,0,1,0}, {0,0,1,0}, {0,0,1,0}, {0,0,1,0} },
                { {0,0,0,0}, {0,0,0,0}, {1,1,1,1}, {0,0,0,0} },
                { {0,1,0,0}, {0,1,0,0}, {0,1,0,0}, {0,1,0,0} }
            },
            // J
            new int[,,] {
                { {1,0,0}, {1,1,1}, {0,0,0} },
                { {0,1,1}, {0,1,0}, {0,1,0} },
                { {0,0,0}, {1,1,1}, {0,0,1} },
                { {0,1,0}, {0,1,0}, {1,1,0} }
            },
            // L
            new int[,,] {
                { {0,0,1}, {1,1,1}, {0,0,0} },
                { {0,1,0}, {0,1,0}, {0,1,1} },
                { {0,0,0}, {1,1,1}, {1,0,0} },
                { {1,1,0}, {0,1,0}, {0,1,0} }
            },
            // O
            new int[,,] {
                { {1,1}, {1,1} }
            },
            // S
            new int[,,] {
                { {0,1,1}, {1,1,0}, {0,0,0} },
                { {0,1,0}, {0,1,1}, {0,0,1} },
                { {0,0,0}, {0,1,1}, {1,1,0} },
                { {1,0,0}, {1,1,0}, {0,1,0} }
            },
            // T
            new int[,,] {
                { {0,1,0}, {1,1,1}, {0,0,0} },
                { {0,1,0}, {0,1,1}, {0,1,0} },
                { {0,0,0}, {1,1,1}, {0,1,0} },
                { {0,1,0}, {1,1,0}, {0,1,0} }
            },
            // Z
            new int[,,] {
                { {1,1,0}, {0,1,1}, {0,0,0} },
                { {0,0,1}, {0,1,1}, {0,1,0} },
                { {0,0,0}, {1,1,0}, {0,1,1} },
                { {1,0,0}, {1,1,0}, {0,1,0} }
            }
        };

        public static void DropPiece()
        {
            var y = 0;
            while (!CollisionDetected(pieceRotate, pieceX, pieceY - y))
            {
                y++;
            }
            y--;
            pieceY -= y;
            FreezePiece();
        }

        public static void FreezePiece()
        {
            var currentPiece = pieces[pieceNo];
            var dim = currentPiece.GetLength(1);
            bool reachedTop = false;

            for (var y = dim - 1; y >= 0; y--)
            {
                for (var x = 0; x < dim; x++)
                {
                    if (currentPiece[pieceRotate, y, x] == 1)
                    {
                        int boardY = pieceY - y;
                        if (boardY >= 0)
                        {
                            well[boardY, pieceX + x] = pieceNo + 1; // Zapisujemy indeks koloru +1
                        }
                    }
                }
            }

            if (reachedTop)
            {
                gameEnded = true; // Koniec gry, gdy klocek nakłada się na górze planszy
                return;
            }

            CompactWell();
            RandomizePiece();

            // Sprawdzenie, czy nowy klocek ma kolizję na starcie
            if (CollisionDetected(pieceRotate, pieceX, pieceY))
            {
                gameEnded = true;
            }
        }

        public static void HandleKey(Key key)
        {
            lock (syncObject)
            {
                if (gameEnded) return;

                keyPressedRecently = false;

                switch (key)
                {
                    case Key.Left:
                        if (!CollisionDetected(pieceRotate, pieceX - 1, pieceY))
                        {
                            pieceX--;
                        }
                        break;
                    case Key.Right:
                        if (!CollisionDetected(pieceRotate, pieceX + 1, pieceY))
                        {
                            pieceX++;
                        }
                        break;
                    case Key.Down:
                        if (!CollisionDetected(pieceRotate, pieceX, pieceY - 1))
                        {
                            pieceY--;
                        }
                        else
                        {
                            FreezePiece();
                        }
                        break;
                    case Key.Up:
                        if (!CollisionDetected((pieceRotate + 1) % pieces[pieceNo].GetLength(0), pieceX, pieceY))
                        {
                            pieceRotate = (pieceRotate + 1) % pieces[pieceNo].GetLength(0);
                        }
                        else if (!CollisionDetected((pieceRotate + 1) % pieces[pieceNo].GetLength(0), pieceX - 1, pieceY))
                        {
                            pieceX -= 1;
                            pieceRotate = (pieceRotate + 1) % pieces[pieceNo].GetLength(0);
                        }
                        else if (!CollisionDetected((pieceRotate + 1) % pieces[pieceNo].GetLength(0), pieceX + 1, pieceY))
                        {
                            pieceX += 1;
                            pieceRotate = (pieceRotate + 1) % pieces[pieceNo].GetLength(0);
                        }
                        else if (!CollisionDetected((pieceRotate + 1) % pieces[pieceNo].GetLength(0), pieceX - 2, pieceY))
                        {
                            pieceX -= 2;
                            pieceRotate = (pieceRotate + 1) % pieces[pieceNo].GetLength(0);
                        }
                        else if (!CollisionDetected((pieceRotate + 1) % pieces[pieceNo].GetLength(0), pieceX + 2, pieceY))
                        {
                            pieceX += 2;
                            pieceRotate = (pieceRotate + 1) % pieces[pieceNo].GetLength(0);
                        }
                        break;
                    case Key.Space:
                        DropPiece();
                        break;
                    case Key.Escape:
                        // koniec gierki
                        gameEnded = true;
                        break;
                    default:
                        return;
                }
                OnRedrawRequested?.Invoke();
                keyPressedRecently = true;
            }
        }

        public static void RandomizePiece()
        {
            pieceNo = upcomingPieces[0];
            pieceRotate = 0;
            pieceX = pieceNo == 3 ? 4 : 3;
            pieceY = pieceYinit;

            upcomingPieces.RemoveAt(0);
            AddNextPieceToUpcoming();
        }

        public static void AddNextPieceToUpcoming()
        {
            if (bagIdx == -1 || bagIdx >= currentBag.Length - 1)
            {
                currentBag = currentBag.OrderBy(x => rnd.Next()).ToArray();
                bagIdx = 0;
            }
            else
            {
                bagIdx++;
            }
            upcomingPieces.Add(currentBag[bagIdx]);
        }

        public static void Init()
        {
            score = 0;
            dropInterval = 1000;

            currentBag = currentBag.OrderBy(x => rnd.Next()).ToArray();
            upcomingPieces.Clear();

            for (int i = 0; i < 5; i++)
            {
                AddNextPieceToUpcoming();
            }

            ResetBoard();
            gameEnded = false;
            keyPressedRecently = false;
        }

        public static void StartNewGame(string player)
        {
            playerName = player;
            Init();
            StartTimer();
        }

        public static void StartTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(dropInterval)
            };
            timer.Tick += Tick;
            timer.Start();
        }

        internal static void Tick(object? sender, EventArgs e)
        {
            if (gameEnded)
            {
                StopGame();
                OnGameOver?.Invoke();
                return;
            }
            if (!keyPressedRecently)
            {
                HandleKey(Key.Down);
            }

            keyPressedRecently = false;

            OnRedrawRequested?.Invoke();
        }

        public static void StopGame()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        public static void CompactWell()
        {
            int lines = 0;
            var y = 0;
            while (y < wellHeight)
            {
                var allFilled = true;
                for (var x = 0; x < wellWidth; x++)
                {
                    if (well[y, x] == 0)
                    {
                        allFilled = false;
                        break;
                    }
                }
                if (!allFilled)
                {
                    y++;
                    continue;
                }

                lines++;
                for (var y2 = y; y2 < wellHeight - 2; y2++)
                {
                    for (var x = 0; x < wellWidth; x++)
                    {
                        well[y2, x] = well[y2 + 1, x];
                    }
                }
                for (var x = 0; x < wellWidth; x++)
                {
                    well[wellHeight - 1, x] = 0;
                }
            }

            switch (lines)
            {
                case 4:
                    score += 1200;
                    break;
                case 3:
                    score += 300;
                    break;
                case 2:
                    score += 100;
                    break;
                case 1:
                    score += 40;
                    break;
            }

            UpdateDropSpeed();
        }

        public static bool CollisionDetected(int pieceNewRotation, int pieceNewX, int pieceNewY)
        {
            var currentPiece = pieces[pieceNo];
            var dim = currentPiece.GetLength(1);

            for (var y = 0; y < dim; y++)
            {
                for (var x = 0; x < dim; x++)
                {
                    // Sprawdzamy tylko pola klocka
                    if (currentPiece[pieceNewRotation, y, x] == 1)
                    {
                        int boardX = pieceNewX + x;
                        int boardY = pieceNewY - y;

                        // Kolizja z granicami planszy
                        if (boardX < 0 || boardX >= wellWidth || boardY < 0 || boardY >= wellHeight)
                            return true;

                        // Kolizja z innymi klockami
                        if (well[boardY, boardX] > 0)
                            return true;
                    }
                }
            }
            return false;
        }

        private static void UpdateDropSpeed()
        {
            if (score >= 10000)
            {
                dropInterval = 100;
            }
            else if (score >= 7500)
            {
                dropInterval = 150;
            }
            else if (score >= 5000)
            {
                dropInterval = 200;
            }
            else if (score >= 3000)
            {
                dropInterval = 400;
            }
            else if (score >= 2000)
            {
                dropInterval = 600;
            }
            else if (score >= 1000)
            {
                dropInterval = 800;
            }

            if (timer != null)
                timer.Interval = TimeSpan.FromMilliseconds(dropInterval);
        }

        public static void SaveScore()
        {
            string filePath = "scoreboard.txt";
            List<string> scores = new List<string>();

            // Wczytanie istniejących wyników, jeśli plik istnieje
            if (File.Exists(filePath))
            {
                scores = File.ReadAllLines(filePath)
                             .Where(line => !string.IsNullOrWhiteSpace(line)) // Pomijamy puste linie
                             .ToList();
            }

            // Dodanie nowego wyniku
            string newScore = $"{playerName}: {score}";
            scores.Add(newScore);

            // Sortowanie wyników malejąco
            var sortedScores = scores
                .Select(s => new { Text = s, Value = ExtractScoreValue(s) })
                .OrderByDescending(s => s.Value)
                .Select(s => s.Text)
                .ToList();

            // Zapisanie wyników z powrotem do pliku
            File.WriteAllLines(filePath, sortedScores);
        }

        private static int ExtractScoreValue(string scoreText)
        {
            if (string.IsNullOrWhiteSpace(scoreText)) return 0;

            var parts = scoreText.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int scoreValue))
            {
                return scoreValue;
            }
            return 0;
        }

        public static void ResetGameState()
        {
            gameEnded = false;
            keyPressedRecently = false;
            score = 0;
            ResetBoard();
            ResetBlocks();
            if (timer != null)
            {
                timer.Stop();
            }
        }

        private static void ResetBoard()
        {
            for (int i = 0; i < wellHeight; i++)
            {
                for (int j = 0; j < wellWidth; j++)
                {
                    well[i, j] = 0;
                }
            }
        }

        private static void ResetBlocks()
        {
            pieceYinit = 24;
            pieceNo = 4;
            pieceRotate = 1;
            pieceX = 4;
            pieceY = pieceYinit;
        }

        public static int CalculateShadowY()
        {
            int shadowY = pieceY;

            while (!CollisionDetected(pieceRotate, pieceX, shadowY - 1))
            {
                shadowY--;
            }

            return shadowY;
        }
    }
}
