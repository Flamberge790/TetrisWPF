using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using tetris.Models;
using tetris.Views;

namespace tetris.Controllers
{
    public static class BlockController
    {
        public static Block _Block = new Block();
        private static object syncObject = new object();
        public static int pieceYinit = 22;
        public static int pieceNo = 4;
        public static int pieceRotate = 1;
        public static int pieceX = 4;
        public static int pieceY = pieceYinit;
        private static int bagIdx = -1;
        public static int[] currentBag = new int[] { 0, 1, 2, 3, 4, 5, 6 };
        public static List<int> upcomingPieces = new List<int>();

        /// <summary>
        /// immediately places the piece
        /// </summary>
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
        
        /// <summary>
        /// responsible for 'freezing' the pieces in place once they reach the bottom (or land on another piece)
        /// </summary>
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
        /// <summary>
        ///  handles the up, down, left, right arrow inputs during an active game of tetris
        /// </summary>
        /// <param name="key"></param>
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


        /// <summary>
        /// chooses a new active piece from the upcomingPieces list, selects its coordinates
        /// based on piece type, ends the game if the new active piece collides with something
        /// upon being added and adds a new piece to the list of upcoming pieces
        /// </summary>
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

        /// <summary>
        /// adds a random piece to the list of upcoming pieces displayed on the right during an active game of tetris
        /// </summary>
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
    }
}
