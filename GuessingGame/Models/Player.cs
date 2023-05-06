using System.ComponentModel.DataAnnotations;

namespace GuessingGame.Models
{
    public class Player
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "The player name must be between 2 and 50 characters long")]
        [Required(ErrorMessage = "The player name is required")]
        public string? Name { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "The total guess must be a positive number")]
        public int TotalGuess { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "The total win must be a positive number")]
        public int TotalWin { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "The total game must be a positive number")]
        public int TotalGame { get; set; }
        public double SuccessRate { get; set; }
    }
}
