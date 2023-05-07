using GuessingGame.DataLayer;
using GuessingGame.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GuessingGame.Services
{
    public class GameService : IGameService
    {
        private readonly GuessingGameDbContext _context;

        public GameService(GuessingGameDbContext context)
        {
            _context = context;
        }
        #region GameModel
        public async Task<Game> StartNewGameAsync(int playerId)
        {
            var game = new Game
            {
                PlayerId = playerId,
                SecretNumber = GenerateSecretNumber(),
                TriesLeft = 8,
                StartTime = DateTime.Now,
                IsOver = false
            };

            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();

            return game;
        }
        private int GenerateSecretNumber()
        {
            var random = new Random();

            while (true)
            {
                var digits = Enumerable.Range(0, 10).ToList();
                var secret = Enumerable.Range(0, 4).Select(_ =>
                {
                    var index = random.Next(digits.Count);
                    var digit = digits[index];
                    digits.RemoveAt(index);
                    return digit;
                }).ToArray();

                if (secret[0] != 0) // ensure first digit is not 0 can be removed if desired.
                {
                    return secret[0] * 1000 + secret[1] * 100 + secret[2] * 10 + secret[3];
                }
            }
        }
        public async Task<Game> GetGameByIdAsync(int gameId)
        {
            var game = await _context.Games
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null)
            {
                throw new ArgumentException($"Game with id {gameId} does not exist.");
            }

            return game;
        }
        public async Task<Game?> GetGameByPlayerIdAsync(int playerId)
        {
            var game = await _context.Games
                    .Where(g => g.PlayerId == playerId)
                    .OrderByDescending(g => g.Id)
                    .FirstOrDefaultAsync();
            if (game == null)
            {
                return null;
            }

            return game;
        }
        #endregion

        #region PlayerModel
        public async Task<Player> CreatePlayerIfNotExistAsync(string? playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                throw new ArgumentException("Player name cannot be null or empty.");
            }

            var existingPlayer = await _context.Players.FirstOrDefaultAsync(p => p.Name == playerName);

            if (existingPlayer != null)
            {
                return existingPlayer;
            }

            var newPlayer = new Player
            {
                Name = playerName,
                TotalGuess = 0,
                TotalWin = 0,
                TotalGame = 0
            };

            _context.Players.Add(newPlayer);
            await _context.SaveChangesAsync();

            return newPlayer;
        }
        public async Task<Player> GetPlayerByIdAsync(int id)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
            {
                throw new ArgumentException($"Player with id {id} does not exist.");
            }

            return player;
        }
        public async Task<List<Player>> GetPlayerRankAsync()
        {
            // Get players who has more than 3 finished games
            var players = await _context.Players
                .Where(p => p.TotalGame >= 3)
                .ToListAsync();

            // Calculate success rate for each player
            foreach (var player in players)
            {
                player.SuccessRate = player.TotalGame >= 3 ? (double)player.TotalWin / player.TotalGame : 0;
                _context.Players.Update(player);
            }
            await _context.SaveChangesAsync();
            // Sort players by success rate and total guesses
            players = players.OrderByDescending(p => p.SuccessRate)
                             .ThenBy(p => p.TotalGuess)
                             .ToList();

            return players;
        }
        #endregion

        #region GuessModel
        public async Task<Guess> MakeGuessAsync(int gameId, int playerId, int guessNumber)
        {
            string Log = string.Empty;
            // Find the game by id and include related Guesses and the corresponding Player
            var player = await _context.Players
                .FirstOrDefaultAsync(g => g.Id == playerId);

            if (player == null)
            {
                throw new InvalidOperationException("Player not found");
            }
            if (player.Id != playerId)
            {
                throw new InvalidOperationException("Current player is not correct.");
            }
            var game = await _context.Games
                .FirstOrDefaultAsync(g => g.Id == gameId && g.PlayerId == playerId);

            if (game == null)
            {
                throw new ArgumentException($"No game found with id {gameId}");
            }

            if (game.IsOver)
            {
                throw new InvalidOperationException("Game is already over");
            }

            var guesses = await _context.Guess
                    .Where(g => g.GameId == gameId && g.PlayerId == playerId)
                    .OrderByDescending(g => g.Id)
                    .ToListAsync();
            if (guesses != null)
            {
                foreach (var item in guesses)
                {
                    Log += item.GuessResult + " Your guess was " + item.GuessNumber + "\n";
                }
            }

            // Calculate the matching digits and places for the current guess
            var matchingDigits = 0;
            var matchingPlaces = 0;
            var secretNumberString = game.SecretNumber.ToString();
            var guessNumberString = guessNumber.ToString();

            for (int i = 0; i < 4; i++)
            {
                if (secretNumberString[i] == guessNumberString[i])
                {
                    matchingPlaces++;
                }
                else if (secretNumberString.Contains(guessNumberString[i]))
                {
                    matchingDigits++;
                }
            }

            // Add the current guess to the game's guesses list
            var guess = new Guess
            {
                GameId = gameId,
                PlayerId = playerId,
                GuessNumber = guessNumber,
                SecretNumber = game.SecretNumber,
                MatchingDigits = matchingDigits,
                MatchingPlaces = matchingPlaces,
                GuessResult = $"M:{matchingDigits}; P:{matchingPlaces}",
                Log = Log,
                GuessTime = DateTime.Now
            };

            // Decrement the tries left and update the game status
            game.TriesLeft--;

            if (matchingPlaces == 4)
            {
                game.IsWon = true;
                game.IsOver = true;
                game.EndTime = DateTime.Now;
                player.TotalWin++;
                player.TotalGame++;
                Log = string.Empty;
            }
            else if (game.TriesLeft == 0)
            {
                game.IsOver = true;
                game.EndTime = DateTime.Now;
                player.TotalGame++;
                Log = string.Empty;
            }

            // Update the player's total guess count for the current game
            player.TotalGuess++;

            // Update the game and save changes to the database
            _context.Games.Update(game);
            _context.Players.Update(player);
            _context.Guess.Add(guess);
            await _context.SaveChangesAsync();

            return guess;
        }
        public async Task<Guess?> GetLastGuessAsync(int playerId, int currentGameId)
        {
            var lastGuess = await _context.Guess
                    .Where(g => g.PlayerId == playerId && g.GameId == currentGameId)
                    .OrderByDescending(g => g.Id)
                    .FirstOrDefaultAsync();
            if (lastGuess == null)
            {
                return null;
            }
            return lastGuess;
        }
        #endregion
    }
}
