using GuessingGame.Models;
using Microsoft.EntityFrameworkCore;

namespace GuessingGame.DataLayer
{
    public class GuessingGameDbContext : DbContext
    {
        public GuessingGameDbContext(DbContextOptions<GuessingGameDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Guess> Guess { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Add any custom model configuration here
            modelBuilder.Entity<Player>().HasKey(p => p.Id);
            modelBuilder.Entity<Game>().HasKey(g => g.Id);
            modelBuilder.Entity<Guess>().HasKey(gu => gu.Id);
        }
    }
}
