namespace GuessingGameTests
{
    public class GameServiceStartNewGameTests
    {
        private IGameService _gameService;
        private DbContextOptions<GuessingGameDbContext> _dbOptions;

        public GameServiceStartNewGameTests()
        {
            _dbOptions = new DbContextOptionsBuilder<GuessingGameDbContext>()
                .UseInMemoryDatabase(databaseName: "GuessingGame")
                .Options;

            _gameService = new GameService(new GuessingGameDbContext(_dbOptions));
        }
        [Fact]
        public async Task StartNewGameAsync_Should_CreateNewGame()
        {
            // Arrange
            var playerId = 1;

            // Act
            var result = await _gameService.StartNewGameAsync(playerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(playerId, result.PlayerId);
            Assert.NotNull(result.SecretNumber);
            Assert.True(result.TriesLeft == 8);
            Assert.True(result.StartTime > DateTime.MinValue);
            Assert.Null(result.EndTime);
            Assert.False(result.IsWon);
            Assert.False(result.IsOver);
        }
        [Fact]
        public async Task StartNewGameAsync_Should_AddNewGameToDatabase()
        {
            // Arrange
            var playerId = 1;

            // Act
            await _gameService.StartNewGameAsync(playerId);

            // Assert
            using (var dbContext = new GuessingGameDbContext(_dbOptions))
            {
                var game = await dbContext.Games.FirstOrDefaultAsync(g => g.PlayerId == playerId);
                Assert.NotNull(game);
            }
        }
    }
}