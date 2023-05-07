namespace GuessingGameTests.GameServiceTests
{
    public class GameServiceMakeGuessTests
    {
        private IGameService _gameService;
        private DbContextOptions<GuessingGameDbContext> _dbOptions;
        private readonly GuessingGameDbContext _context;

        public GameServiceMakeGuessTests()
        {
            _dbOptions = new DbContextOptionsBuilder<GuessingGameDbContext>()
                .UseInMemoryDatabase(databaseName: "GuessingGame")
                .Options;
            _context = new GuessingGameDbContext(_dbOptions);
            _gameService = new GameService(new GuessingGameDbContext(_dbOptions));
        }

        [Fact]
        public async Task MakeGuessAsync_GameNotFound_ThrowsArgumentException()
        {
            // Arrange
            var gameId = 1;
            var playerId = 1;
            var guessNumber = 1234;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _gameService.MakeGuessAsync(gameId, playerId, guessNumber));
        }

        [Fact]
        public async Task MakeGuessAsync_GameIsOver_ThrowsInvalidOperationException()
        {
            // Arrange
            var gameId = 1;
            var playerId = 1;
            var guessNumber = 1234;

            var game = new Game
            {
                PlayerId = playerId,
                IsOver = true
            };
            _context.Games.Add(game);
            _context.SaveChanges();


            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _gameService.MakeGuessAsync(gameId, playerId, guessNumber));
        }

        [Fact]
        public async Task MakeGuessAsync_WrongPlayer_ThrowsInvalidOperationException()
        {
            // Arrange
            var gameId = 1;
            var playerId = 1;
            var guessNumber = 1234;

            var game = new Game
            {
                PlayerId = playerId + 1
            };

            _context.Games.Add(game);
            _context.SaveChanges();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _gameService.MakeGuessAsync(gameId, playerId, guessNumber));
        }

        [Fact]
        public async Task MakeGuessAsync_GuessIsCorrect_ReturnsGuessWithMatchingDigitsAndPlaces()
        {
            // Arrange
            var playerName = "Ahmet";
            var secretNumber = 1234;
            var guessNumber = secretNumber;

            var player = new Player
            {
                Name = playerName,
                TotalGame = 0,
                TotalGuess = 0,
                TotalWin = 0
            };

            _context.Players.Add(player);
            _context.SaveChanges();

            var game = new Game
            {
                PlayerId = player.Id,
                SecretNumber = secretNumber,
                TriesLeft = 1,
                IsOver = false
            };


            _context.Games.Add(game);
            _context.SaveChanges();

            // Act
            var guess = await _gameService.MakeGuessAsync(game.Id, player.Id, guessNumber);

            // Assert
            Assert.Equal(secretNumber, guess.GuessNumber);
            Assert.Equal(4, guess.MatchingPlaces);
        }
        [Fact]
        public async Task MakeGuessAsync_GameWon_IsWonFieldIsTrue()
        {
            // Arrange
            var Name = "Ahmet";
            int secretNumber = 1234;
            int guessNumber = 1234;
            var player = new Player
            {
                Name = Name,
                TotalGame = 0,
                TotalGuess = 0,
                TotalWin = 0
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            var game = new Game
            {
                PlayerId = player.Id,
                SecretNumber = secretNumber,
                TriesLeft = 1,
                IsOver = false
            };

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            // Act
            var guess = await _gameService.MakeGuessAsync(game.Id, player.Id, guessNumber);

            // Assert
            using (var context = new GuessingGameDbContext(_dbOptions))
            {
                var updatedGame = await context.Games.FirstOrDefaultAsync(g => g.Id == game.Id);
                Assert.True(updatedGame.IsWon);
            }
        }
        [Fact]
        public async Task GameService_MakeGuessAsync_CalculatesTotalsCorrectly()
        {
            // Arrange
            var player = new Player
            {
                Name = "Hasan",
                TotalGame = 0,
                TotalGuess = 0,
                TotalWin = 0
            };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            var game = new Game
            {
                SecretNumber = 1234,
                TriesLeft = 1,
                IsOver = false,
                IsWon = false,
                PlayerId = player.Id
            };
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            // Act
            await _gameService.MakeGuessAsync(game.Id, game.PlayerId, 5678);

            // Assert
            using (var context = new GuessingGameDbContext(_dbOptions))
            {
                var players = await context.Players.FirstOrDefaultAsync(p => p.Id == game.PlayerId);
                Assert.NotNull(players);
                Assert.Equal(1, players.TotalGuess);
                Assert.Equal(0, players.TotalWin);
                Assert.Equal(1, players.TotalGame);

            }
        }

        [Fact]
        public async Task GetLastGuessAsync_ShouldReturnLastGuess_WhenLastGuessExist()
        {
            // Arrange
            var playerId = 1;
            var currentGameId = 1;
            var expectedGuess = new Guess { PlayerId = playerId, GameId = currentGameId, GuessNumber = 5910 };
            _context.Guess.Add(expectedGuess);
            await _context.SaveChangesAsync();

            // Act
            var result = await _gameService.GetLastGuessAsync(playerId, currentGameId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedGuess.PlayerId, result.PlayerId);
            Assert.Equal(expectedGuess.GameId, result.GameId);
            Assert.Equal(expectedGuess.GuessNumber, result.GuessNumber);
        }

        [Fact]
        public async Task GetLastGuessAsync_ShouldReturnNull_WhenLastGuessDoesNotExist()
        {
            // Arrange
            var playerId = 1;
            var currentGameId = 1;

            // Act
            var result = await _gameService.GetLastGuessAsync(playerId, currentGameId);

            // Assert
            Assert.Null(result);
        }
    }
}