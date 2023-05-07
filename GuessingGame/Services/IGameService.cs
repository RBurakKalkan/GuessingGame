using GuessingGame.Models;

namespace GuessingGame.Services
{
    public interface IGameService
    {
        //Game
        Task<Game> StartNewGameAsync(int playerId);//new SecretNumber will be generated.
        Task<Game> GetGameByIdAsync(int gameId);
        Task<Game?> GetGameByPlayerIdAsync(int playerId);

        ////Player
        Task<Player> CreatePlayerIfNotExistAsync(string? playerName);
        Task<Player> GetPlayerByIdAsync(int Id);
        Task<List<Player>> GetPlayerRankAsync();

        ////Guess
        Task<Guess> MakeGuessAsync(int gameId, int playerId, int GuessNumber); // GuessNumber will be compared to SecretNumber according to the GameRules.
        Task<Guess?> GetLastGuessAsync(int playerId, int currentGameId);
    }
}
