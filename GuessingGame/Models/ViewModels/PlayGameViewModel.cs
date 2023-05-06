namespace GuessingGame.Models.ViewModels
{
    public class PlayGameViewModel
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public int TrialsLeft { get; set; }
        public int MatchingDigits  { get; set; }
        public int MatchingPlaces  { get; set; }
        public int GuessNumber  { get; set; }
        public string? Logs { get; set; }
        public string? GuessResult { get; set; }
        public Boolean IsOver { get; set; }
    }
}
