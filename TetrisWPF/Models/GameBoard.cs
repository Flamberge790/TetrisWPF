using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tetris.Models
{
    public static class GameBoard
    {
        public static int wellWidth = 10;
        public static int wellHeight = 25;
        public static int[,] well = new int[wellHeight, wellWidth];
    }
}
