
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using tetris.Controllers;

namespace tetris.Models
{
    public class Block
    {
        public readonly List<int[,,]> pieces;
        private static object syncObject = new object();
        public static int pieceYinit = 22;
        public static int pieceNo = 4;
        public static int pieceRotate = 1;
        public static int pieceX = 4;
        public static int pieceY = pieceYinit;
        private static int bagIdx = -1;
        public static int[] currentBag = new int[] { 0, 1, 2, 3, 4, 5, 6 };
        public static List<int> upcomingPieces = new List<int>();

        public static Random rnd = new Random();
        static Timer timer;
        static int dropInterval = 1000;
        public static bool gameEnded = false;
        public static bool keyPressedRecently = false;

        public Block()
        {
            pieces = new List<int[,,]> {
            new int[,,] {{ {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} },
                         { {0,0,1,0}, {0,0,1,0}, {0,0,1,0}, {0,0,1,0} },
                         { {0,0,0,0}, {0,0,0,0}, {1,1,1,1}, {0,0,0,0} },
                         { {0,1,0,0}, {0,1,0,0}, {0,1,0,0}, {0,1,0,0} }},

            new int[,,] {{ {1,0,0}, {1,1,1}, {0,0,0} },
                         { {0,1,1}, {0,1,0}, {0,1,0} },
                         { {0,0,0}, {1,1,1}, {0,0,1} },
                         { {0,1,0}, {0,1,0}, {1,1,0} }},

            new int[,,] {{ {0,0,1}, {1,1,1}, {0,0,0} },
                         { {0,1,0}, {0,1,0}, {0,1,1} },
                         { {0,0,0}, {1,1,1}, {1,0,0} },
                         { {1,1,0}, {0,1,0}, {0,1,0} }},

            new int[,,] {{ {1,1}, {1,1} }},

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

        }

        public static class GameBoard
        {
            public static int wellWidth = 10;
            public static int wellHeight = 25;
            public static int[,] well = new int[wellHeight, wellWidth];
        }

        public static class Player
        {
            public static string playerName = "";
            public static int score = 0;
        }

        public static void HandleMainMenu(int option)
        {
            while (true)
            {
                switch (option)
                {
                    case 1:
                        GameStateController.StartNewGame();
                        break;
                    case 2:
                        ScoreBoard.ShowScoreboard();
                        break;
                    case 3:
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("\nNieprawidłowy wybór, spróbuj ponownie.");
                        MainMenu.ShowMainMenu();
                        break;
                }
            }
        }

        private static void DropPiece()
        {
            var y = 0;
            while (false == GameStateController.CollisionDetected(pieceRotate, pieceX, pieceY - y))
            {
                y++;
            }
            y--;
            pieceY -= y;
            FreezePiece();
        }

        private static void FreezePiece()
        {
            var currentPiece = _Block.pieces[pieceNo];
            var dim = currentPiece.GetLength(1); // always square

            for (var y = dim - 1; y >= 0; y--)
            {
                for (var x = 0; x < dim; x++)
                {
                    if (currentPiece[pieceRotate, y, x] == 1)
                    {
                        GameBoard.well[pieceY - y, pieceX + x] = 1;
                    }
                    if (pieceY - y >= GameBoard.wellHeight)
                    {
                        GameStateController.gameEnded = true;
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

            GameStateController.CompactWell();
            RandomizePiece();

        }

        public static void HandleKey(ConsoleKey key)
        {
            lock (syncObject)
            {
                switch (key)
                {
                    case ConsoleKey.LeftArrow:
                        if (false == GameStateController.CollisionDetected(pieceRotate, pieceX - 1, pieceY))
                        {
                            pieceX--;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (false == GameStateController.CollisionDetected(pieceRotate, pieceX + 1, pieceY))
                        {
                            pieceX++;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (false == GameStateController.CollisionDetected(pieceRotate, pieceX, pieceY - 1))
                        {
                            pieceY--;
                        }
                        else
                        {
                            FreezePiece();

                        }

                        break;
                    case ConsoleKey.UpArrow:
                        if (false == GameStateController.CollisionDetected((pieceRotate + 1) % _Block.pieces[pieceNo].GetLength(0), pieceX, pieceY))
                        {
                            pieceRotate = (pieceRotate + 1) % _Block.pieces[pieceNo].GetLength(0);
                        }
                        else if (false == GameStateController.CollisionDetected((pieceRotate + 1) % _Block.pieces[pieceNo].GetLength(0), pieceX - 1, pieceY))
                        {
                            pieceX -= 1;
                            pieceRotate = (pieceRotate + 1) % _Block.pieces[pieceNo].GetLength(0);
                        }
                        else if (false == GameStateController.CollisionDetected((pieceRotate + 1) % _Block.pieces[pieceNo].GetLength(0), pieceX + 1, pieceY))
                        {
                            pieceX += 1;
                            pieceRotate = (pieceRotate + 1) % _Block.pieces[pieceNo].GetLength(0);
                        }
                        else if (false == GameStateController.CollisionDetected((pieceRotate + 1) % _Block.pieces[pieceNo].GetLength(0), pieceX - 2, pieceY))
                        {
                            pieceX -= 2;
                            pieceRotate = (pieceRotate + 1) % _Block.pieces[pieceNo].GetLength(0);

                        }
                        else if (false == GameStateController.CollisionDetected((pieceRotate + 1) % _Block.pieces[pieceNo].GetLength(0), pieceX + 2, pieceY))
                        {
                            pieceX += 2;
                            pieceRotate = (pieceRotate + 1) % _Block.pieces[pieceNo].GetLength(0);
                        }

                        break;
                    case ConsoleKey.Spacebar:
                        DropPiece();
                        break;
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                }
                GameStateView.DrawEverything();
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
                currentBag = currentBag.OrderBy(x => GameStateController.rnd.Next()).ToArray();
                bagIdx = 0;
            }
            else
            {
                bagIdx++;
            }
            upcomingPieces.Add(currentBag[bagIdx]);
        }

        public static void MainDisplay()
        {
            Console.SetWindowSize(90, 30);
            MainMenu.ShowMainMenu();
        }

        public static void Init()
        {
            Player.score = 0;
            dropInterval = 1000;

            BlockController.currentBag = BlockController.currentBag.OrderBy(x => rnd.Next()).ToArray();
            BlockController.upcomingPieces.Clear();

            for (int i = 0; i < 5; i++)
            {
                BlockController.AddNextPieceToUpcoming();
            }
        }

        public static void StartNewGame()
        {
            Console.Clear();
            Console.WriteLine("Podaj swoją nazwę: ");
            Player.playerName = Console.ReadLine();
            Init();
            MainLoop();
        }

        private static void Tick(object state)
        {
            if (false == gameEnded && false == keyPressedRecently)
            {
                BlockController.HandleKey(ConsoleKey.DownArrow);
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
                BlockController.HandleKey(key.Key);
            }
            ResetGameState();
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                SaveScore();
                Thread.Sleep(1000);


                MainMenu.ShowMainMenu();
            }
        }

        public static void CompactWell()
        {

            int lines = 0;

            var y = 0;
            while (y < GameBoard.wellHeight)
            {
                var allFilled = true;
                for (var x = 0; x < GameBoard.wellWidth; x++)
                {
                    if (GameBoard.well[y, x] == 0)
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

                for (var y2 = y; y2 < GameBoard.wellHeight - 2; y2++)
                {
                    for (var x = 0; x < GameBoard.wellWidth; x++)
                    {
                        GameBoard.well[y2, x] = GameBoard.well[y2 + 1, x];
                    }
                }
                for (var x = 0; x < GameBoard.wellWidth; x++)
                {
                    GameBoard.well[GameBoard.wellHeight - 1, x] = 0;
                }


            }

            switch (lines)
            {
                case 4:
                    Player.score += 1200;
                    break;
                case 3:
                    Player.score += 300;
                    break;
                case 2:
                    Player.score += 100;
                    break;
                case 1:
                    Player.score += 40;
                    break;
            }

            UpdateDropSpeed();
        }

        public static bool CollisionDetected(int pieceNewRotation, int pieceNewX, int pieceNewY)
        {
            var currentPiece = BlockController._Block.pieces[BlockController.pieceNo];
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
                    if (currentPiece[pieceNewRotation, y, x] == 1 && pieceNewX + x >= GameBoard.wellWidth)
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
                    if (currentPiece[pieceNewRotation, y, x] == 1 && GameBoard.well[pieceNewY - y, pieceNewX + x] == 1)
                    {
                        return true;
                    }

                }
            }
            return false;


        }

        private static void UpdateDropSpeed()
        {
            if (Player.score >= 10000)
            {
                dropInterval = 100; // maksymalne przyspieszenie
            }
            if (Player.score >= 7500)
            {
                dropInterval = 150;
            }
            if (Player.score >= 5000)
            {
                dropInterval = 200;
            }
            else if (Player.score >= 3000)
            {
                dropInterval = 400;
            }
            else if (Player.score >= 2000)
            {
                dropInterval = 600;
            }
            else if (Player.score >= 1000)
            {
                dropInterval = 800;
            }

            // Aktualizacja timera z nowym odstępem czasowym
            timer.Change(0, dropInterval);
        }

        public static void SaveScore()
        {
            string result = $"{Player.playerName}: {Player.score}";
            File.AppendAllText("scoreboard.txt", result + Environment.NewLine);
            Console.SetCursorPosition(50, 9);
            Console.WriteLine("Wynik zapisany!");
        }

        public static void ResetGameState()
        {
            gameEnded = false;
            keyPressedRecently = false;
            Player.score = 0;

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
            for (int i = 0; i < GameBoard.wellHeight; i++)
            {
                for (int j = 0; j < GameBoard.wellWidth; j++)
                {
                    GameBoard.well[i, j] = 0;
                }
            }
        }

        private static void ResetBlocks()
        {
            BlockController.pieceYinit = 22;
            BlockController.pieceNo = 4;
            BlockController.pieceRotate = 1;
            BlockController.pieceX = 4;
            BlockController.pieceY = 22;
        }
    }

}
