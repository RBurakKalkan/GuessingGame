using GuessingGame.Models;
using GuessingGame.Models.ViewModels;
using GuessingGame.Services;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

public class GameController : Controller
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }
    [HttpGet]
    public async Task<IActionResult> StartGame(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            ModelState.AddModelError("playerName", "Please enter a valid player name.");
            return View();
        }

        var player = await _gameService.CreatePlayerIfNotExistAsync(playerName);

        var game = await _gameService.GetGameByPlayerIdAsync(player.Id);

        var model = new PlayGameViewModel();

        if (game != null && !game.IsOver)
        {
            var lastGuess = await _gameService.GetLastGuessAsync(player.Id, game.Id);
            model.GameId = game.Id;
            model.PlayerId = player.Id;
            model.PlayerName = player.Name;
            model.TrialsLeft = game.TriesLeft;
            if (lastGuess != null)
            {
                model.GuessResult = lastGuess.GuessResult;
                model.Logs = lastGuess.Log;
                model.MatchingPlaces = lastGuess.MatchingPlaces;
                model.MatchingDigits = lastGuess.MatchingDigits;
                model.GuessNumber = lastGuess.GuessNumber;
            }
            return View("PlayGame", model);
        }
        else
        {
            game = await _gameService.StartNewGameAsync(player.Id);
        }


        model = new PlayGameViewModel
        {
            GameId = game.Id,
            PlayerName = player.Name,
            PlayerId = player.Id,
            TrialsLeft = game.TriesLeft,
            IsOver = game.IsOver,
        };

        return View("PlayGame", model);
    }
    [HttpPost]
    public async Task<IActionResult> MakeGuess(int gameId, int guessNumber)
    {
        var game = await _gameService.GetGameByIdAsync(gameId);

        if (game == null)
        {
            return NotFound();
        }

        var result = await _gameService.MakeGuessAsync(gameId, game.PlayerId, guessNumber);

        var model = new PlayGameViewModel
        {
            GameId = game.Id,
            GuessResult = result.GuessResult,
            Logs = result.Log,
            TrialsLeft = game.TriesLeft,
            MatchingDigits = result.MatchingDigits,
            MatchingPlaces = result.MatchingPlaces,
            IsOver = game.IsOver,
            GuessNumber = guessNumber
        };

        return Json(model);
    }
    [HttpGet]
    public async Task<IActionResult> FinishGame(int gameId, string Name)
    {
        var game = await _gameService.GetGameByIdAsync(gameId);

        if (game == null)
        {
            return NotFound();
        }

        var model = new FinishGameViewModel
        {
            GameId = game.Id,
            IsWon = game.IsWon,
            PlayerName = Name,
            SecretNumber = game.SecretNumber
        };

        return View("FinishGame", model);
    }
    [HttpGet]
    public async Task<IActionResult> Leaderboard()
    {
        var playerRank = await _gameService.GetPlayerRankAsync();

        var viewModel = new PlayerRankViewModel
        {
            Players = playerRank
        };

        return View("Leaderboard", viewModel);
    }
}
