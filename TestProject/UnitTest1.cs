using System.Numerics;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using TetrisWPF;

namespace TestProject
{
    public class ModelTests
    {
        [SetUp]
        public void Setup()
        {
            
            for (int y = 0; y < TetrisModel.wellHeight; y++)
            {
                for (int x = 0; x < TetrisModel.wellWidth; x++)
                {
                    TetrisModel.well[y, x] = 0;
                }
            }

            TetrisModel.playerName = "";
            TetrisModel.score = 0;


        }

        [Test]
        public void GameBoard_InitialWell_ShouldBeEmpty()
        {
            for (int y = 0; y < TetrisModel.wellHeight; y++)
            {
                for (int x = 0; x < TetrisModel.wellWidth; x++)
                {
                    Assert.AreEqual(0, TetrisModel.well[y, x], $"Pole [{y},{x}] powinno byæ puste.");
                }
            }
        }

        [Test]
        public void Player_InitialScore_ShouldBeZero()
        {
            Assert.AreEqual(0, TetrisModel.score, "Pocz¹tkowy wynik powinien byæ równy 0.");
        }

        [Test]
        public void Player_SetName_ShouldUpdateName()
        {
            TetrisModel.playerName = "TestPlayer";
            Assert.AreEqual("TestPlayer", TetrisModel.playerName, "Nazwa gracza powinna byæ zaktualizowana.");
        }

        private Block block;

        [Test]
        public void Block_ShouldContainSevenPieces()
        {
            Assert.AreEqual(7, TetrisModel.pieces.Count, "Powinno byæ dok³adnie 7 rodzajów klocków.");
        }

        [Test]
        public void Block_PieceDimensions_ShouldBeValid()
        {
            foreach (var piece in TetrisModel.pieces)
            {
                int dim = piece.GetLength(1); // Rozmiar kawa³ka
                Assert.AreEqual(dim, piece.GetLength(2), "Kszta³t ka¿dego klocka powinien byæ kwadratowy.");
            }
        }
    }

    public class ControllerTests
    {
        [SetUp]
        public void Setup()
        {
            // Reset planszy gry przed ka¿dym testem
            for (int y = 0; y < TetrisModel.wellHeight; y++)
            {
                for (int x = 0; x < TetrisModel.wellWidth; x++)
                {
                    TetrisModel.well[y, x] = 0;
                }
            }

            TetrisModel.score = 0;
            TetrisModel.playerName = "";
            TetrisModel.upcomingPieces.Clear();
        }

        [Test]
        public void CollisionDetected_Should_ReturnTrue_WhenCollisionWithBottom()
        {
            // Arrange
            TetrisModel.pieceNo = 0; // Klocek I
            TetrisModel.pieceX = 4;
            TetrisModel.pieceY = 0;

            // Act
            bool collision = TetrisModel.CollisionDetected(0, TetrisModel.pieceX, TetrisModel.pieceY - 1);

            // Assert
            Assert.IsTrue(collision); // Kolizja powinna zostać wykryta
        }

        [Test]
        public void CollisionDetected_Should_ReturnFalse_WhenNoCollision()
        {
            // Arrange
            TetrisModel.pieceNo = 0; // Klocek I
            TetrisModel.pieceX = 4;
            TetrisModel.pieceY = 20;

            // Act
            bool collision = TetrisModel.CollisionDetected(0, TetrisModel.pieceX, TetrisModel.pieceY - 1);

            // Assert
            Assert.IsFalse(collision); // Kolizji nie powinno być
        }

        [Test]
        public void BlockController_ShouldAddNextPieceToUpcoming()
        {
            // Arrange
            Assert.IsEmpty(TetrisModel.upcomingPieces, "Lista nadchodz¹cych klocków powinna byæ pusta.");

            // Act
            TetrisModel.AddNextPieceToUpcoming();

            // Assert
            Assert.AreEqual(1, TetrisModel.upcomingPieces.Count, "Lista nadchodz¹cych klocków powinna zawieraæ jeden element.");
        }

        [Test]
        public void GameStateController_Init_ShouldResetState()
        {
            // Arrange
            TetrisModel.score = 100;
            TetrisModel.upcomingPieces.Add(1);

            // Act
            TetrisModel.Init();

            // Assert
            Assert.AreEqual(0, TetrisModel.score, "Wynik powinien byæ zresetowany do 0.");
            Assert.AreEqual(5, TetrisModel.upcomingPieces.Count, "Lista nadchodz¹cych klocków powinna zawieraæ 5 elementów po inicjalizacji.");
        }
    }
}