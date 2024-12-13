using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tetris.Views;

namespace tetris.Controllers
{
    public static class MainMenuController
    {
        /// <summary>
        /// calls a method to start the game, show the scoreboard or exit the app based on 
        /// the user's choice in the main menu
        /// </summary>
        /// <param name="option"></param>
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
    }
}
