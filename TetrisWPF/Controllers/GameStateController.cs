using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using tetris.Models;
using tetris.Views;

namespace tetris.Controllers
{
    public static class GameStateController
    {
        public static Random rnd = new Random();
        static Timer timer;
        static int dropInterval = 1000;
        public static bool gameEnded = false;
        public static bool keyPressedRecently = false;
        public static void MainDisplay()
        {
            Console.SetWindowSize(90, 30);
            MainMenu.ShowMainMenu();
        }
        /// <summary>
        /// initializes a new game of tetris, reseting the score to 0, clearing the list 
        /// of upcoming pieces from the previous game and filling it back with a 
        /// new, full set of 5 random pieces
        /// </summary>
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
        /// <summary>
        /// called upon selecting "start gry" from the main menu, asks for the player name,
        /// sets the name, calls for the game to be initialized then starts the main loop
        /// </summary>
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

        /// <summary>
        /// clears full lines and increases score based on number of lines cleared at once
        /// </summary>
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

        /// <summary>
        /// checks for collisions with the left and right wall of the board (well), the bottom
        /// and other already placed pieces. if true then the piece cannot move or rotate to 
        /// a given position
        /// </summary>
        /// <param name="pieceNewRotation"></param>
        /// <param name="pieceNewX"></param>
        /// <param name="pieceNewY"></param>
        /// <returns></returns>
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

        /// <summary>
        /// increases the speed at which blocks fall while no input is detected, based on score
        /// </summary>
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

        /// <summary>
        /// saves the score to a text file
        /// </summary>
        public static void SaveScore() 
        {
            string result = $"{Player.playerName}: {Player.score}";
            File.AppendAllText("scoreboard.txt", result + Environment.NewLine);
            Console.SetCursorPosition(50, 9);
            Console.WriteLine("Wynik zapisany!");
        }

        /// <summary>
        /// reset the game state in order to prepare the application for the start of a new game.
        /// ResetBlocks() sets the piece coordinates, position and type to the default values
        /// ResetTimer() Disposes of the currenty active timer which is later created anew in the MainLoop
        /// ResetBoard() clears the board
        /// </summary>
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
