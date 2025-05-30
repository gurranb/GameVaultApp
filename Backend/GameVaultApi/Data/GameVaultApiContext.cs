using GameVaultApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace GameVaultApi.Data
{
    public class GameVaultApiContext : DbContext
    {
        public GameVaultApiContext(DbContextOptions<GameVaultApiContext> options)
                : base(options)
        {
        }

        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<CachedOwnedGames> CachedOwnedGames { get; set; }
        public DbSet<OwnedGamesUser> OwnedGamesUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Add custom configs if needed
        }
    }
}
