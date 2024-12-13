using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace TetrisWPF
{
    public class TetrisModel
    {
        //block
        private static object syncObject = new object();
        public static int pieceYinit = 22;
        public static int pieceNo = 4;
        public static int pieceRotate = 1;
        public static int pieceX = 4;
        public static int pieceY = pieceYinit;
        private static int bagIdx = -1;
        public static int[] currentBag = new int[] { 0, 1, 2, 3, 4, 5, 6 };
        public static List<int> upcomingPieces = new List<int>();

        //gamestate
        public static Random rnd = new Random();
        static Timer timer;
        static int dropInterval = 1000;
        public static bool gameEnded = false;
        public static bool keyPressedRecently = false;

        //gameboard
        public static int wellWidth = 10;
        public static int wellHeight = 25;
        public static int[,] well = new int[wellHeight, wellWidth];

        //player
        public static string playerName = "";
        public static int score = 0;

        public static readonly List<int[,,]> pieces = new List<int[,,]> {
            new int[,,] {{ {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} },
                         { {0,0,1,0}, {0,0,1,0}, {0,0,1,0}, {0,0,1,0} },
                         { {0,0,0,0}, {0,0,0,0}, {1,1,1,1}, {0,0,0,0} },
                         { {0,1,0,0}, {0,1,0,0}, {0,1,0,0}, {0,1,0,0} }},

            new int[,,] {{ {1,0,0}, { 1,1,1}, { 0,0,0} },
                         { { 0,1,1}, { 0,1,0}, { 0,1,0} },
                         { { 0,0,0}, { 1,1,1}, { 0,0,1} },
                         { { 0,1,0}, { 0,1,0}, { 1,1,0} }},

            new int[,,] {{ {0,0,1}, {1,1,1}, {0,0,0} },
                         { {0,1,0}, {0,1,0}, {0,1,1} },
                         { {0,0,0}, {1,1,1}, {1,0,0} },
                         { {1,1,0}, {0,1,0}, {0,1,0} }},

            new int[,,] { { { 1, 1 }, { 1, 1 } } },

            new int[,,] {{ {0,1,1}, {1,1,0}, {0,0,0} },
                         { {0,1,0}, {0,1,1}, {0,0,1} },
                         { {0,0,0}, {0,1,1}, {1,1,0} },
                         { {1,0,0}, {1,1,0}, {0,1,0} }},

            new int[,,] {{ {0,1,0}, {1,1,1}, {0,0,0} },
                         { {0,1,0}, {0,1,1}, {0,1,0} },
                         { {0,0,0}, {1,1,1}, {0,1,0} },
                         { {0,1,0}, {1,1,0}, {0,1,0} }},

            new int[,,] {{ {1,1,0}, {0,1,1}, {0,0,0} },
                         { {0,0,1}, {0,1,1}, {0,1,0} },
                         { {0,0,0}, {1,1,0}, {0,1,1} },
                         { {1,0,0}, {1,1,0}, {0,1,0} }}
        };

        private static void DropPiece()
        {
            var y = 0;
            while (false == CollisionDetected(pieceRotate, pieceX, pieceY - y))
            {
                y++;
            }
            y--;
            pieceY -= y;
            FreezePiece();
        }

        private static void FreezePiece()
        {
            var currentPiece = pieces[pieceNo];
            var dim = currentPiece.GetLength(1); // always square

            for (var y = dim - 1; y >= 0; y--)
            {
                for (var x = 0; x < dim; x++)
                {
                    if (currentPiece[pieceRotate, y, x] == 1)
                    {
                        well[pieceY - y, pieceX + x] = 1;
                    }
                    if (pieceY - y >= wellHeight)
                    {
                        gameEnded = true;
                    }

                }
            }

            int startX = 50; // Adjust start position if necessary
            int startY = 13; // Adjust starting y-position if necessary
            for (int i = 0; i < 5 * 4; i++) // Clears an area big enough for 5 blocks with spacing
            {
                Console.SetCursorPosition(startX, startY + i);
                Console.Write("          "); // Clear line
            }

            CompactWell();
            RandomizePiece();

        }

        public static void HandleKey(ConsoleKey key)
        {
            lock (syncObject)
            {
                switch (key)
                {
                    case ConsoleKey.LeftArrow:
                        if (false == CollisionDetected(pieceRotate, pieceX - 1, pieceY))
                        {
                            pieceX--;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (false == CollisionDetected(pieceRotate, pieceX + 1, pieceY))
                        {
                            pieceX++;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (false == CollisionDetected(pieceRotate, pieceX, pieceY - 1))
                        {
                            pieceY--;
                        }
                        else
                        {
                            FreezePiece();

                        }

                        break;
                    case ConsoleKey.UpArrow:
                        if (false == CollisionDetected((pieceRotate + 1) % pieces[pieceNo].GetLength(0), pieceX, pieceY))
                        {
                            pieceRotate = (pieceRotate + 1) % pieces[pieceNo].GetLength(0);
                        }
                        else if (false == CollisionDetected((pieceRotate + 1) % pieces[pieceNo].GetLength(0), pieceX - 1, pieceY))
                        {
                            pieceX -= 1;
                            pieceRotate = (pieceRotate + 1) % pieces[pieceNo].GetLength(0);
                        }
                        else if (false == CollisionDetected((pieceRotate + 1) % pieces[pieceNo].GetLength(0), pieceX + 1, pieceY))
                        {
                            pieceX += 1;
                            pieceRotate = (pieceRotate + 1) % pieces[pieceNo].GetLength(0);
                        }
                        else if (false == CollisionDetected((pieceRotate + 1) % pieces[pieceNo].GetLength(0), pieceX - 2, pieceY))
                        {
                            pieceX -= 2;
                            pieceRotate = (pieceRotate + 1) % pieces[pieceNo].GetLength(0);

                        }
                        else if (false == CollisionDetected((pieceRotate + 1) % pieces[pieceNo].GetLength(0), pieceX + 2, pieceY))
                        {
                            pieceX += 2;
                            pieceRotate = (pieceRotate + 1) % pieces[pieceNo].GetLength(0);
                        }

                        break;
                    case ConsoleKey.Spacebar:
                        DropPiece();
                        break;
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        public static void RandomizePiece()
        {
            pieceNo = upcomingPieces[0];
            pieceRotate = 0;
            pieceX = pieceNo == 3 ? 4 : 3;
            pieceY = pieceYinit;

            // Remove used piece and add a new one to the upcoming list
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
        }

        public static void StartNewGame()
        {
            Console.Clear();
            Console.WriteLine("Podaj swoją nazwę: ");
            playerName = Console.ReadLine();
            Init();
            MainLoop();
        }

        private static void Tick(object state)
        {
            if (false == gameEnded && false == keyPressedRecently)
            {
                HandleKey(ConsoleKey.DownArrow);
            }

            keyPressedRecently = false;
        }

        public static void MainLoop()
        {
            Console.Clear();
            timer = new Timer(Tick, null, 0, dropInterval);

            while (!gameEnded)
            {
                var key = Console.ReadKey();
                keyPressedRecently = true;
                HandleKey(key.Key);
            }
            ResetGameState();
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                SaveScore();
                Thread.Sleep(1000);
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
                if (false == allFilled)
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
            var dim = currentPiece.GetLength(1); // always square

            Console.SetCursorPosition(60, 12);

            // left 
            for (var y = dim - 1; y >= 0; y--)
            {
                for (var x = 0; x < dim - 1; x++)
                {
                    if (currentPiece[pieceNewRotation, y, x] == 1 && pieceNewX + x < 0)
                    {
                        //Console.Write($"collision left newX={pieceNewX} x={x}");
                        return true;
                    }
                }
            }

            // right
            for (var y = dim - 1; y >= 0; y--)
            {
                for (var x = dim - 1; x >= 0; x--)
                {
                    if (currentPiece[pieceNewRotation, y, x] == 1 && pieceNewX + x >= wellWidth)
                    {
                        return true;
                    }
                }
            }

            // well bottom (y = 0)
            for (var y = dim - 1; y >= 0; y--)
            {
                for (var x = 0; x < dim; x++)
                {
                    if (currentPiece[pieceNewRotation, y, x] == 1 && pieceNewY - y < 0)
                    {
                        return true;
                    }

                }
            }

            for (var y = dim - 1; y >= 0; y--)
            {
                for (var x = 0; x < dim; x++)
                {
                    if (currentPiece[pieceNewRotation, y, x] == 1 && well[pieceNewY - y, pieceNewX + x] == 1)
                    {
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
                dropInterval = 100; // maksymalne przyspieszenie
            }
            if (score >= 7500)
            {
                dropInterval = 150;
            }
            if (score >= 5000)
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

            // Aktualizacja timera z nowym odstępem czasowym
            timer.Change(0, dropInterval);
        }

        public static void SaveScore()
        {
            string result = $"{playerName}: {score}";
            File.AppendAllText("scoreboard.txt", result + Environment.NewLine);
            Console.SetCursorPosition(50, 9);
            Console.WriteLine("Wynik zapisany!");
        }

        public static void ResetGameState()
        {
            gameEnded = false;
            keyPressedRecently = false;
            score = 0;

            ResetBoard();
            ResetBlocks();
            ResetTimer();
        }

        private static void ResetTimer()
        {
            if (timer != null)
            {
                timer.Dispose();
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
            pieceYinit = 22;
            pieceNo = 4;
            pieceRotate = 1;
            pieceX = 4;
            pieceY = 22;
        }
    }
}
