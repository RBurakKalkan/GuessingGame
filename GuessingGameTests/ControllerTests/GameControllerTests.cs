using GuessingGame.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GuessingGameTests.ControllerTests
{
    public class GameControllerTests
    {
        private readonly Mock<IGameService> _mockGameService;

        public GameControllerTests()
        {
            _mockGameService = new Mock<IGameService>();
        }

        [Fact]
        public async Task StartGame_WithValidPlayerName_ReturnsPlayGameView()
        {
            // Arrange
            var controller = new GameController(_mockGameService.Object);
            var playerName = "John Doe";
            _mockGameService.Setup(s => s.CreatePlayerIfNotExistAsync(playerName))
                            .ReturnsAsync(new Player { Id = 1, Name = playerName });
            _mockGameService.Setup(s => s.GetGameByPlayerIdAsync(1))
                            .ReturnsAsync(new Game { Id = 1, PlayerId = 1, IsOver = false, TriesLeft = 5 });

            // Act
            var result = await controller.StartGame(playerName);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PlayGameViewModel>(viewResult.ViewData.Model);
            Assert.Equal(playerName, model.PlayerName);
            Assert.Equal(5, model.TrialsLeft);
        }

        [Fact]
        public async Task StartGame_WithInvalidPlayerName_ReturnsViewWithModelError()
        {
            // Arrange
            var controller = new GameController(_mockGameService.Object);
            var playerName = "";
            controller.ModelState.AddModelError("playerName", "Please enter a valid player name.");

            // Act
            var result = await controller.StartGame(playerName);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.False(viewResult.ViewData.ModelState.IsValid);
        }

        [Fact]
        public async Task StartExistingGame_WhenPreviousGame_IsNotOver()
        {
            // Arrange
            var playerId = 1;
            var playerName = "Test Player";
            var gameId = 1;
            var game = new Game { Id = gameId, PlayerId = playerId, TriesLeft = 5, IsOver = false };
            var lastGuess = new Guess { GuessNumber = 1234, Log = "Test Log", MatchingDigits = 0, MatchingPlaces = 0 };
            _mockGameService.Setup(x => x.CreatePlayerIfNotExistAsync(playerName)).ReturnsAsync(new Player { Id = playerId, Name = playerName });
            _mockGameService.Setup(x => x.GetGameByPlayerIdAsync(playerId)).ReturnsAsync(game);
            _mockGameService.Setup(x => x.GetLastGuessAsync(playerId, gameId)).ReturnsAsync(lastGuess);

            var controller = new GameController(_mockGameService.Object);

            // Act
            var result = await controller.StartGame(playerName) as ViewResult;
            var model = result.Model as PlayGameViewModel;

            // Assert
            Assert.Equal(gameId, model.GameId);
            Assert.Equal(playerId, model.PlayerId);
            Assert.Equal(playerName, model.PlayerName);
            Assert.Equal(game.TriesLeft, model.TrialsLeft);
            Assert.Equal(lastGuess.GuessResult, model.GuessResult);
            Assert.Equal(lastGuess.Log, model.Logs);
            Assert.Equal(lastGuess.MatchingPlaces, model.MatchingPlaces);
            Assert.Equal(lastGuess.MatchingDigits, model.MatchingDigits);
            Assert.Equal(lastGuess.GuessNumber, model.GuessNumber);
            Assert.Equal("PlayGame", result.ViewName);
        }

        [Fact]
        public async Task StartNewGame_WhenPreviousGame_IsOver()
        {
            // Arrange
            var playerId = 1;
            var playerName = "Test Player";
            var gameId = 1;
            var game = new Game { Id = gameId, PlayerId = playerId, TriesLeft = 5, IsOver = true };
            _mockGameService.Setup(x => x.CreatePlayerIfNotExistAsync(playerName)).ReturnsAsync(new Player { Id = playerId, Name = playerName });
            _mockGameService.Setup(x => x.GetGameByPlayerIdAsync(playerId)).ReturnsAsync(game);
            _mockGameService.Setup(x => x.StartNewGameAsync(playerId)).ReturnsAsync(new Game { Id = 2, PlayerId = playerId, TriesLeft = 5, IsOver = false });

            var controller = new GameController(_mockGameService.Object);

            // Act
            var result = await controller.StartGame(playerName) as ViewResult;
            var model = result.Model as PlayGameViewModel;

            // Assert
            Assert.Equal(2, model.GameId);
            Assert.Equal(playerId, model.PlayerId);
            Assert.Equal(playerName, model.PlayerName);
            Assert.Equal(5, model.TrialsLeft);
            Assert.False(model.IsOver);
            Assert.Equal("PlayGame", result.ViewName);
        }

        [Fact]
        public async Task MakeGuess_ReturnsNotFound_WhenGameIsNull()
        {
            // Arrange
            var controller = new GameController(_mockGameService.Object);
            int gameId = 1;
            int guessNumber = 1234;
            _mockGameService.Setup(service => service.GetGameByIdAsync(gameId))
                .ReturnsAsync((Game)null);

            // Act
            var result = await controller.MakeGuess(gameId, guessNumber);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task MakeGuess_ReturnsJson_WithCorrectModel()
        {
            // Arrange
            var controller = new GameController(_mockGameService.Object);
            int gameId = 1;
            int guessNumber = 1234;
            var game = new Game { Id = gameId, PlayerId = 1, TriesLeft = 10, IsOver = false };
            var guessResult = new Guess { GuessNumber = guessNumber, MatchingDigits = 4, MatchingPlaces = 0 };
            _mockGameService.Setup(service => service.GetGameByIdAsync(gameId))
                .ReturnsAsync(game);
            _mockGameService.Setup(service => service.MakeGuessAsync(gameId, game.PlayerId, guessNumber))
                .ReturnsAsync(guessResult);

            // Act
            var result = await controller.MakeGuess(gameId, guessNumber);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsType<PlayGameViewModel>(jsonResult.Value);

            // Assert
            Assert.Equal(game.Id, model.GameId);
            Assert.Equal(guessResult.GuessResult, model.GuessResult);
            Assert.Equal(guessResult.Log, model.Logs);
            Assert.Equal(game.TriesLeft, model.TrialsLeft);
            Assert.Equal(guessResult.MatchingDigits, model.MatchingDigits);
            Assert.Equal(guessResult.MatchingPlaces, model.MatchingPlaces);
            Assert.Equal(game.IsOver, model.IsOver);
            Assert.Equal(guessNumber, model.GuessNumber);
        }

        [Fact]
        public async Task FinishGame_WithValidGameId_ShouldReturnCorrectViewModel()
        {
            // Arrange
            int gameId = 1;
            string playerName = "John Doe";
            var game = new Game
            {
                Id = gameId,
                IsWon = true,
                SecretNumber = 1234
            };
            _mockGameService.Setup(x => x.GetGameByIdAsync(gameId)).ReturnsAsync(game);
            var controller = new GameController(_mockGameService.Object);

            // Act
            var result = await controller.FinishGame(gameId, playerName) as ViewResult;
            var model = result?.Model as FinishGameViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("FinishGame", result.ViewName);
            Assert.NotNull(model);
            Assert.Equal(gameId, model.GameId);
            Assert.Equal(playerName, model.PlayerName);
            Assert.Equal(game.IsWon, model.IsWon);
            Assert.Equal(game.SecretNumber, model.SecretNumber);
        }

        [Fact]
        public async Task FinishGame_WithInvalidGameId_ShouldReturnNotFound()
        {
            // Arrange
            int gameId = 1;
            string playerName = "John Doe";
            _mockGameService.Setup(x => x.GetGameByIdAsync(gameId)).ReturnsAsync((Game)null);
            var controller = new GameController(_mockGameService.Object);

            // Act
            var result = await controller.FinishGame(gameId, playerName);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Leaderboard_ReturnsView_WithViewModel()
        {
            // Arrange
            var players = new List<Player>
            {
                new Player { Name = "Alice", TotalWin = 3 },
                new Player { Name = "Bob", TotalWin = 2 },
                new Player { Name = "Charlie", TotalWin = 1 },
            };
            _mockGameService.Setup(mock => mock.GetPlayerRankAsync())
                            .ReturnsAsync(players);
            var controller = new GameController(_mockGameService.Object);

            // Act
            var result = await controller.Leaderboard();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsAssignableFrom<PlayerRankViewModel>(viewResult.ViewData.Model);
            Assert.Equal(players, viewModel.Players);
        }

        [Fact]
        public async Task Leaderboard_ReturnsLeaderboardView()
        {
            // Arrange
            var controller = new GameController(_mockGameService.Object);

            // Act
            var result = await controller.Leaderboard();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Leaderboard", viewResult.ViewName);
        }

    }
}
