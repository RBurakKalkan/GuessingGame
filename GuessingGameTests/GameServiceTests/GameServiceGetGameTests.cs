namespace GuessingGameTests.GameServiceTests.GameServiceTests
{
    public class GameServiceGetGameTests
    {
        private IGameService _gameService;
        private DbContextOptions<GuessingGameDbContext> _dbOptions;
        private readonly GuessingGameDbContext _context;

        public GameServiceGetGameTests()
        {
            _dbOptions = new DbContextOptionsBuilder<GuessingGameDbContext>()
                .UseInMemoryDatabase(databaseName: "GuessingGame")
                .Options;
            _context = new GuessingGameDbContext(_dbOptions);
            _gameService = new GameService(new GuessingGameDbContext(_dbOptions));
        }

        [Fact]
        public async Task GetGameByIdAsync_GameExists_ReturnsGame()
        {
            // Arrange
            var game = new Game { StartTime = DateTime.Now };
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            // Act
            var result = await _gameService.GetGameByIdAsync(game.Id);

            // Assert
            Assert.Equal(game.Id, result.Id);
        }

        [Fact]
        public async Task GetGameByIdAsync_GameDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            var gameId = 9999;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _gameService.GetGameByIdAsync(gameId));
        }

        [Fact]
        public async Task GetGameByPlayerIdAsync_GameExists_ReturnsGame()
        {
            // Arrange
            var playerId = 1;
            var game1 = new Game { PlayerId = playerId };
            var game2 = new Game { PlayerId = playerId };
            _context.Games.AddRange(game1, game2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _gameService.GetGameByPlayerIdAsync(playerId);

            // Assert
            Assert.Equal(game2.Id, result.Id);
        }

        [Fact]
        public async Task GetGameByPlayerIdAsync_GameDoesNotExist_ReturnsNull()
        {
            // Arrange
            var playerId = 10;

            // Act
            var result = await _gameService.GetGameByPlayerIdAsync(playerId);

            // Assert
            Assert.Null(result);
        }
    }
}
