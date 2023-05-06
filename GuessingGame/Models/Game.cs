using System.ComponentModel.DataAnnotations;

namespace GuessingGame.Models
{
    public class Game
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The PlayerId is required")]
        public int PlayerId { get; set; }

        [Range(1000, 9999, ErrorMessage = "The secret number must be between 1000 and 9999")]
        [Required(ErrorMessage = "The secret number is required")]
        public int SecretNumber { get; set; }

        [Range(0, 8, ErrorMessage = "The TriesLeft must be between 0 and 8")]
        [Required(ErrorMessage = "The TriesLeft is required")]
        public int TriesLeft { get; set; }
        public bool IsWon { get; set; }
        public bool IsOver { get; set; }

        [Required(ErrorMessage = "The StartTime is required")]
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}