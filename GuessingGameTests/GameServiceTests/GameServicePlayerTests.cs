
using GuessingGame.Models;

namespace GuessingGameTests.GameServiceTests
{
    public class GameServicePlayerTests
    {
        private IGameService _gameService;
        private DbContextOptions<GuessingGameDbContext> _dbOptions;
        private readonly GuessingGameDbContext _context;

        public GameServicePlayerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<GuessingGameDbContext>()
                .UseInMemoryDatabase(databaseName: "GuessingGame")
                .Options;
            _context = new GuessingGameDbContext(_dbOptions);
            _gameService = new GameService(new GuessingGameDbContext(_dbOptions));
        }

        [Fact]
        public async Task CreatePlayerIfNotExistAsync_ShouldCreateNewPlayer_WhenPlayerDoesNotExist()
        {
            // Arrange
            var playerName = "John";

            // Act
            var result = await _gameService.CreatePlayerIfNotExistAsync(playerName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(playerName, result.Name);
            Assert.Equal(0, result.TotalGuess);
            Assert.Equal(0, result.TotalWin);
            Assert.Equal(0, result.TotalGame);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task GetPlayerByIdAsync_ShouldReturnPlayer_WhenPlayerExists()
        {
            // Arrange
            var playerName = "John";
            var newPlayer = new Player
            {
                Name = playerName,
                TotalGuess = 0,
                TotalWin = 0,
                TotalGame = 0
            };
            _context.Players.Add(newPlayer);
            await _context.SaveChangesAsync();

            // Act
            var result = await _gameService.GetPlayerByIdAsync(newPlayer.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(playerName, result.Name);
        }
        [Fact]
        public async Task CreatePlayerIfNotExistAsync_ShouldReturnExistingPlayer_WhenPlayerExist()
        {
            // Arrange
            var existingPlayer = new Player
            {
                Name = "John",
                TotalGuess = 0,
                TotalWin = 0,
                TotalGame = 0
            };
            _context.Players.Add(existingPlayer);
            await _context.SaveChangesAsync();

            // Act
            var result = await _gameService.CreatePlayerIfNotExistAsync(existingPlayer.Name);

            // Assert
            Assert.NotNull(result);
            //Assert.Equal(existingPlayer.Id, result.Id);
            Assert.Equal(existingPlayer.Name, result.Name);
            Assert.Equal(existingPlayer.TotalGuess, result.TotalGuess);
            Assert.Equal(existingPlayer.TotalWin, result.TotalWin);
            Assert.Equal(existingPlayer.TotalGame, result.TotalGame);
        }
        [Fact]
        public async Task GetPlayerRankAsync_ReturnsPlayersSortedBySuccessRateAndTotalGuess()
        {
            // Arrange
            var players = new List<Player>
                 {
                     new Player { Name = "Player 1", TotalGame = 5, TotalWin = 3, TotalGuess = 20, SuccessRate = 0 },
                     new Player { Name = "Player 2", TotalGame = 6, TotalWin = 1, TotalGuess = 10, SuccessRate = 0 },
                     new Player { Name = "Player 3", TotalGame = 4, TotalWin = 2, TotalGuess = 15, SuccessRate = 0 },
                     new Player { Name = "Player 4", TotalGame = 7, TotalWin = 1, TotalGuess = 5, SuccessRate = 0 },
                 };
            _context.Players.AddRange(players);
            _context.SaveChanges();

            // Act
            var result = await _gameService.GetPlayerRankAsync();

            // Assert
            Assert.Equal(4, result.Count);

            Assert.Equal("Player 1", result[0].Name);
            Assert.Equal(3.0 / 5, result[0].SuccessRate);
            Assert.Equal(20, result[0].TotalGuess);
            Assert.Equal(5, result[0].TotalGame);

            Assert.Equal("Player 3", result[1].Name);
            Assert.Equal(2.0 / 4, result[1].SuccessRate);
            Assert.Equal(15, result[1].TotalGuess);
            Assert.Equal(4, result[1].TotalGame);

            Assert.Equal("Player 2", result[2].Name);
            Assert.Equal(1.0 / 6, result[2].SuccessRate);
            Assert.Equal(10, result[2].TotalGuess);
            Assert.Equal(6, result[2].TotalGame);

            Assert.Equal("Player 4", result[3].Name);
            Assert.Equal(1.0 / 7, result[3].SuccessRate);
            Assert.Equal(5, result[3].TotalGuess);
            Assert.Equal(7, result[3].TotalGame);
        }


    }
}
