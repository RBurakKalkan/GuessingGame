namespace GuessingGame.Models.ViewModels
{
    public class FinishGameViewModel
    {
        public int GameId { get; set; }
        public bool IsWon { get; set; }
        public int SecretNumber { get; set; }
        public string? PlayerName { get; set; }
    }
}
