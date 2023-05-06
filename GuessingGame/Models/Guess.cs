using System.ComponentModel.DataAnnotations;

namespace GuessingGame.Models
{
    public class Guess
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GameId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [Range(1000, 9999, ErrorMessage = "Guess number must be between 1000 and 9999.")]
        public int GuessNumber { get; set; }

        public int SecretNumber { get; set; }

        [Range(0, 4, ErrorMessage = "Matching digits must be between 0 and 4.")]
        public int MatchingDigits { get; set; }

        [Range(0, 4, ErrorMessage = "Matching places must be between 0 and 4.")]
        public int MatchingPlaces { get; set; }

        public string? GuessResult { get; set; }

        public string? Log { get; set; }

        [Required]
        public DateTime GuessTime { get; set; }
    }
}
